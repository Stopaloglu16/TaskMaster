using System.Net;
using System.Reflection;
using Hangfire;
using Microsoft.AspNetCore.Http.HttpResults;
using WebApiEmailService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddHangfire(config =>
{
    config.UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("SqlServerConnection"));
});

builder.Services.AddHangfireServer();

builder.Services.AddScoped<IJobTestService, JobTestService>();
builder.Services.AddScoped<IJobReportService, JobReportService>();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");


app.MapGet("/jobrun", async (IJobTestService _jobService, IBackgroundJobClient _backgroundJobClient) =>
{
   
   _backgroundJobClient.Enqueue( () => _jobService.FireAndForgetJob());

    return "success";

});

app.MapPost("/jobcreate", async (IJobTestService _jobService, 
                                        IRecurringJobManager _recurringJobManager) =>
{
    //RecurringJobOptions options = new RecurringJobOptions
    //{
    //    TimeZone = TimeZoneInfo.Local

    //};

    _recurringJobManager.AddOrUpdate(
        "Recurring_Job",
        () => _jobService.RecurringJob(), 

        "0 8-17 * * *", 
        TimeZoneInfo.Local
     );

    return HttpStatusCode.Created;
});

app.MapPost("/jobreportcreate", async (IJobReportService _jobReportService,
                                        IRecurringJobManager _recurringJobManager) =>
{
    //RecurringJobOptions options = new RecurringJobOptions
    //{
    //    TimeZone = TimeZoneInfo.Local

    //};

    _recurringJobManager.AddOrUpdate(
        "Recurring_Job",
        () => _jobReportService.SendReportAsync(),

        "*/15 8-17 * * *",
        TimeZoneInfo.Local
     );

    return HttpStatusCode.Created;
});

app.UseHangfireDashboard();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
