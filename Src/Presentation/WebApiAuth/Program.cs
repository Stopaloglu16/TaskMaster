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


builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<WebIdentityContext>();

builder.Services.AddScoped(typeof(IRepository<,>), typeof(EfCoreRepository<,>));

builder.Services.AddUserServices();


var appSettings = new AppSettings();
builder.Configuration.Bind(nameof(AppSettings), appSettings);

//Email sender setup
builder.Services.AddTransient<IEmailSender>(provider =>
{
    return new EmailSender(appSettings.MailinatorApiToken, appSettings.MailinatorDomain);
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


//app.MapHealthChecks("/health");

app.Run();



public partial class Program { }