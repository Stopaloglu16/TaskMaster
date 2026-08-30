using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TickerQ.DependencyInjection;
using TickerQ.EntityFrameworkCore.DbContextFactory;
using TickerQ.EntityFrameworkCore.DependencyInjection;
using WebApi.Apis;
using WebApi.Extensions;
using WebApi.Middlewares;
using WebApi.Notification;
using WebApi.RabbitMq;

var builder = WebApplication.CreateBuilder(args);

// Add services DI
builder.AddServiceDefaults();
builder.AddApplicationServices();
builder.Services.AddProblemDetails();
builder.Services.AddAuthorizationBuilder();

var configuration = builder.Configuration;


//builder.Services.AddAuthentication(options =>
//{
//    // JWT Bearer 
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//})
//.AddJwtBearer(options =>
//{

//    options.Authority = "https://securetoken.google.com/yourprojectid";
//    options.Audience = "yourprojectid";
//});


builder.Services.AddOpenTelemetry()
    .WithMetrics(m => m.AddMeter("TaskManagerMetrics"))
    .WithTracing(m => m.AddSource("TaskManager"));

// Add API versioning and explorer
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new QueryStringApiVersionReader("api-version"),
        new HeaderApiVersionReader("X-Version")
    );
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddSingleton<IIdempotencyStore, InMemoryIdempotencyStore>();


builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddHostedService<TaskProcessingWorker>();

builder.Services.AddSignalR();
builder.Services.AddHealthChecks();



// Registers IConnection for the "rabbitmq" resource wired in the AppHost, plus its health check.
builder.AddRabbitMQClient("rabbitmq");

builder.Services.AddSingleton<ResultStore>();
// Publisher and consumer both go through RabbitQueues.TaskListBulk, so the queue name and
// durability cannot drift apart. The consumer does the CreateTaskListBulk work and pushes
// "TaskCompleted" to the requestId group on TaskProgressHub.
builder.Services.AddSingleton<RabbitPublisher>();
builder.Services.AddHostedService<RabbitConsumer>();


// TickerQ builds its own DbContextOptions, so it reads the Aspire-injected
// connection string directly rather than going through AddNpgsqlDbContext.
var sqlCon = configuration.GetConnectionString("taskmasterdb");

//"/tickerq-dashboard"
builder.Services.AddTickerQ(options =>
{
    options.AddOperationalStore(efOptions =>
    {
        //efOptions.SetDbContextPoolSize(34);

        efOptions.UseTickerQDbContext<TickerQDbContext>(optionsBuilder =>
        {
            optionsBuilder.UseNpgsql(sqlCon,
                cfg =>
                {
                    cfg.MigrationsAssembly("WebApi");
                    cfg.EnableRetryOnFailure(3);
                });
        }, schema: "ticker");
    });
});


// Enable middleware to serve generated Swagger as a JSON endpoint.
//builder.Services.AddSwaggerGen(options =>
//{
//    // Add JWT Bearer definition
//    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//    {
//        Name = "Authorization",
//        Type = SecuritySchemeType.Http,
//        Scheme = "bearer",
//        BearerFormat = "JWT",
//        In = ParameterLocation.Header,
//        Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6...\""
//    });

//    // Add global security requirement
//    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
//    {
//        [new OpenApiSecuritySchemeReference("bearer", document)] = []
//    });

//    // (Optional) If you use API versioning, set up Swagger docs per version here
//});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

#region WriteIntoFile
#if !DEBUG
Log.Logger = new LoggerConfiguration()
    //.WriteTo.Console()
    .WriteTo.File("Logs/WebApiLog.txt", rollingInterval: RollingInterval.Day)
    .MinimumLevel.Warning()
    .CreateLogger();

builder.Host.UseSerilog();
#endif
#endregion


var app = builder.Build();

// Apply the TickerQ migrations (schema "ticker"). WebApi owns them — see
// MigrationsAssembly("WebApi") above — and WebApiAuth cannot: it has no TickerQ reference.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Migrations");

    try
    {
        // UseTickerQDbContext registers a pooled factory; the context it hands back is ours to dispose.
        var tickerDbFactory = services.GetService<IDbContextFactory<TickerQDbContext>>();

        if (tickerDbFactory != null)
        {
            await using var tickerDb = await tickerDbFactory.CreateDbContextAsync();
            await tickerDb.Database.MigrateAsync();
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating the TickerQ database.");
        throw;
    }
}

app.MapDefaultEndpoints();


//var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();


    // Enable middleware to serve generated Swagger as a JSON endpoint.
    //app.UseSwagger();
    ////app.UseSwaggerUI();
    //app.UseSwaggerUI(options =>
    //{
    //    foreach (var description in provider.ApiVersionDescriptions)
    //    {
    //        options.SwaggerEndpoint(
    //            $"/swagger/{description.GroupName}/swagger.json",
    //            description.GroupName.ToUpperInvariant());
    //    }
    //});
}


// Ahead of the endpoint mappings below, so the pipeline reads in the order it runs. (Endpoint
// routing made the old placement — after every MapGroup — work anyway, but it reads as a bug.)
app.UseAuthentication();
app.UseAuthorization();


// Register versioned APIs using the versioned API explorer
var apiVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1, 0))
    .HasApiVersion(new ApiVersion(2, 0))
    .ReportApiVersions()
    .Build();

var taskList = app.MapGroup("api/v{apiVersion:apiVersion}/tasklist")
    .WithApiVersionSet(apiVersionSet)
    .HasApiVersion(1.0)
    .HasApiVersion(2.0);
taskList.TaskListApiV1().RequireAuthorization();


var taskItem = app.MapGroup("api/v{apiVersion:apiVersion}/taskitem")
    .WithApiVersionSet(apiVersionSet)
    .HasApiVersion(1.0)
    .HasApiVersion(2.0);
taskItem.TaskItemApiV1().RequireAuthorization();

var dashboard = app.MapGroup("api/v{apiVersion:apiVersion}/dashboard")
    .WithApiVersionSet(apiVersionSet)
    .HasApiVersion(1.0)
    .HasApiVersion(2.0);
dashboard.DashboardApiV1().RequireAuthorization();

var fileUpload = app.MapGroup("api/v{apiVersion:apiVersion}/fileupload")
    .WithApiVersionSet(apiVersionSet)
    .HasApiVersion(1.0)
    .HasApiVersion(2.0);
fileUpload.FileUploadApiV1().RequireAuthorization();

// Test email API without authorization for now
/* var sendEmail = app.MapGroup("api/v{apiVersion:apiVersion}/sendemail")
    .WithApiVersionSet(apiVersionSet)
    .HasApiVersion(1.0);

sendEmail.EmailApiV1(); */

var taskPriority = app.MapGroup("api/v{apiVersion:apiVersion}/taskPriority")
    .WithApiVersionSet(apiVersionSet)
    .HasApiVersion(1.0);
taskPriority.TaskPriorityApiV1().RequireAuthorization();



var advancedSearch = app.MapGroup("api/v{apiVersion:apiVersion}/advancedSearch")
    .WithApiVersionSet(apiVersionSet)
    .HasApiVersion(1.0);
advancedSearch.AdvancedSearchApiV1().RequireAuthorization();


app.MapHub<TaskProgressHub>("processHub");
app.MapHub<TaskProgressHub>("processTickerQ");


// Global error handling
app.UseGlobalExceptionHandler();


app.MapHealthChecks("_health");


app.UseTickerQ();

app.UseHttpsRedirection();

app.MapHub<TaskProgressHub>("notifications");

app.Run();


public partial class Program { }