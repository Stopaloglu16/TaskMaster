using Application.Common.Interfaces;
using Application.Common.Models;
using Infrastructure.Abstractions;
using Infrastructure.Data;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using TaskMaster.ServiceDefaults;
using WebApiAuth.Config;
using WebApiAuth.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();


var configuration = builder.Configuration;

// ApplicationDbContext takes ICurrentUserService as well as its options, so it cannot be
// pooled — register it by hand and Enrich* for the Aspire health checks/tracing/retries.
var postgresConnection = configuration.GetConnectionString("taskmasterdb");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(postgresConnection,
        x => x.MigrationsAssembly(@"Infrastructure.PostgresMigrations")));
builder.EnrichNpgsqlDbContext<ApplicationDbContext>(settings => settings.DisableRetry = true);

builder.Services.AddDbContext<WebIdentityContext>(options =>
    options.UseNpgsql(postgresConnection,
        x => x.MigrationsAssembly(@"Infrastructure.PostgresMigrations")));
builder.EnrichNpgsqlDbContext<WebIdentityContext>(settings => settings.DisableRetry = true);

builder.Services.AddScoped(typeof(IApplicationDbContext), typeof(ApplicationDbContext));

// ASP.NET Identity is gone: Keycloak owns credentials, roles and lockout (the realm's brute-force
// detection is configured with the same 5-attempts/5-minutes policy that used to live here).
// WebIdentityContext stays registered above so its tables and migration history remain valid.

builder.Services.AddScoped(typeof(IRepository<,>), typeof(EfCoreRepository<,>));


builder.Services.AddUserServices();

builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

var appSettings = new AppSettings();
builder.Configuration.Bind(nameof(AppSettings), appSettings);

//Email sender setup
builder.Services.AddTransient<IEmailSender>(provider =>
{
    // Use the strongly-typed AppSettings for values, and validate for nulls
    var websiteUrl = appSettings.WebSiteUrl ?? throw new InvalidOperationException("WebSiteUrl is not configured.");
    var apiToken = appSettings.MailinatorApiToken ?? throw new InvalidOperationException("MailinatorApiToken is not configured.");
    var domain = appSettings.MailinatorDomain ?? throw new InvalidOperationException("MailinatorDomain is not configured.");

    return new EmailSender(websiteUrl, apiToken, domain);

});


// Removing AddIdentity took the last registered authentication scheme with it, and the pipeline
// still calls UseAuthentication(). Registering the same Keycloak bearer scheme WebApi uses keeps
// that valid and means UsersController's [Authorize(Roles = "AdminUser")] would now work if
// uncommented — with AddIdentity it never could, since nothing validated the tokens it issued.
builder.AddDefaultAuthentication();

builder.Services.Configure<KeycloakOptions>(builder.Configuration.GetSection(KeycloakOptions.SectionName));

// Typed client for the realm's token endpoint and Admin REST API. The base address is the pinned
// http://localhost:8080 rather than the service-discovery name, so the issuer inside every token
// matches what WebApi validates against.
builder.Services.AddHttpClient<IKeycloakClient, KeycloakClient>((provider, client) =>
{
    var options = provider.GetRequiredService<IOptions<KeycloakOptions>>().Value;

    if (string.IsNullOrWhiteSpace(options.BaseUrl))
        throw new InvalidOperationException("Keycloak:BaseUrl is not configured.");

    client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
});

builder.Services.AddHealthChecks();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

var withApiVersioning = builder.Services.AddApiVersioning();
builder.AddDefaultOpenApi(withApiVersioning);

//Version Configuration
//builder.Services.AddApiVersioning();

//builder.Services.AddAuthorization();
//builder.Services.AddEndpointsApiExplorer();


var app = builder.Build();

// Ensure databases are migrated (optional but recommended)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Seed");

    try
    {
        // Migrate identity and application DBs if you have separate contexts
        var identityDb = services.GetService<WebIdentityContext>();

        if (identityDb != null)
        {
            await identityDb.Database.MigrateAsync();
        }

        var appDb = services.GetService<ApplicationDbContext>();
        if (appDb != null)
        {
            await appDb.Database.MigrateAsync();
        }

        // Seeds the domain User rows only; the Keycloak realm import owns the accounts and
        // passwords, and supplies the AspId values these rows are linked by.
        await SeedData.InitializeAsync(app.Services);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
        throw;
    }
}

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.UseHsts();


app.Run();



public partial class Program { }