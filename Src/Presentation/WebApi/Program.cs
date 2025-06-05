using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using WebApi.Apis;
using WebApi.Extensions;
using WebApi.Notification;

var builder = WebApplication.CreateBuilder(args);

// Add services DI
builder.AddServiceDefaults();
builder.AddApplicationServices();
builder.Services.AddProblemDetails();
builder.Services.AddAuthorizationBuilder();



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


builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddHostedService<TaskProcessingWorker>();

builder.Services.AddSignalR();
builder.Services.AddHealthChecks();
builder.Services.AddSwaggerGen();


var app = builder.Build();

app.MapDefaultEndpoints();


var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    //app.UseSwaggerUI();
    app.UseSwaggerUI(options =>
    {
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                description.GroupName.ToUpperInvariant());
        }
    });
}


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


app.MapHub<TaskProgressHub>("processHub");

//app.MapGet("/connectforecast", () =>
//{

//    string returnMessage = "";

//    IConfigurationRoot configuration = new ConfigurationBuilder()
//         .SetBasePath(Directory.GetCurrentDirectory())
//         .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
//         .Build();

//    // Read the connection string from the configuration
//    string connectionString = configuration.GetConnectionString("SqlServerConnection");


//    // The SQL query you want to execute
//    string sqlQuery = "SELECT [FullName] FROM [TaskMaster].[dbo].[Users]";

//    using (SqlConnection connection = new SqlConnection(connectionString))
//    {
//        try
//        {
//            // Open the connection
//            connection.Open();
//            returnMessage = "Successfully connected to the database.";

//            // Create a SQL command object
//            using (SqlCommand command = new SqlCommand(sqlQuery, connection))
//            {
//                // Execute the query and get a data reader
//                using (SqlDataReader reader = command.ExecuteReader())
//                {
//                    // Check if there are any rows returned
//                    if (reader.HasRows)
//                    {
//                        while (reader.Read())
//                        {
//                            returnMessage += reader["FullName"].ToString();
//                        }

//                    }
//                    else
//                    {
//                        Console.WriteLine("\nNo rows were returned by the query.");
//                    }
//                }
//            }
//        }
//        catch (SqlException ex)
//        {
//            returnMessage = $"Error connecting to or querying the database: {ex.Message}";
//        }
//        finally
//        {
//            // Ensure the connection is closed, even if an error occurred
//            if (connection.State == System.Data.ConnectionState.Open)
//            {
//                connection.Close();
//                Console.WriteLine("Connection closed.");
//            }
//        }
//    }
//    return returnMessage;

//})
//.WithName("ConnectForecast");



app.MapHealthChecks("_health");

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.MapHub<TaskProgressHub>("notifications");

app.Run();


public partial class Program { }