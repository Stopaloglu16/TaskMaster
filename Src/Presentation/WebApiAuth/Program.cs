using Application.Common.Interfaces;
using Application.Common.Models;
using Infrastructure.Abstractions;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskMaster.ServiceDefaults;
using WebApiAuth.Config;
using WebApiAuth.Models;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();


var configuration = builder.Configuration;
var provider = builder.Configuration.GetValue("Provider", "SqlServer");

builder.Services.AddDbContext<ApplicationDbContext>(options => _ = provider switch
{
    "Sqlite" => options.UseSqlite(
        configuration.GetConnectionString("SqliteConnection"),
    x => x.MigrationsAssembly(@"Infrastructure.SqliteMigrations")),

    "SqlServer" => options.UseSqlServer(
        configuration.GetConnectionString("SqlServerConnection"),
    x => x.MigrationsAssembly(@"Infrastructure.SqlServerMigrations")),

    _ => throw new Exception($"Unsupported provider: {provider}")
});


builder.Services.AddDbContext<WebIdentityContext>(options => _ = provider switch
{
    "Sqlite" => options.UseSqlite(
        configuration.GetConnectionString("SqliteConnection"),
    x => x.MigrationsAssembly(@"Infrastructure.SqliteMigrations")),

    "SqlServer" => options.UseSqlServer(
        configuration.GetConnectionString("SqlServerConnection"),
    x => x.MigrationsAssembly(@"Infrastructure.SqlServerMigrations")),

    _ => throw new Exception($"Unsupported provider: {provider}")
});

builder.Services.AddScoped(typeof(IApplicationDbContext), typeof(ApplicationDbContext));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultProvider;
})
.AddEntityFrameworkStores<WebIdentityContext>()
.AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    // Lock the account for 5 minutes if there are 5 failed login attempts
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;

    options.Lockout.AllowedForNewUsers = false;
});

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


{
    var services = builder.Services;
    // configure strongly typed settings object
    services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
}

//var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

//JWT token config
//builder.Services.AddJtwToken(jwtSettings);

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

        // Run the seeder (reads UserManager/RoleManager from DI)
        var seedPassword = builder.Configuration?["Seed:Password"];
        await SeedData.InitializeAsync(app.Services, seedPassword);
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