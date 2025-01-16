using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using TaskMasterRazorClassLibrary.Services;
using WebApp.Config;
using WebApp.Data;
using WebApp.Handlers;
using WebApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

var appSettingSection = builder.Configuration.GetSection("AppSettings");
builder.Services.Configure<ApiSettingConfig>(appSettingSection);

builder.Services.AddTransient<ValidateHeaderHandler>();

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

builder.Services.AddScoped<IAuthService, AuthService>(); // Authentication service

builder.Services.AddScoped<IToastService, ToastService>();

builder.Services.AddBlazorServices();

builder.Services.AddBlazoredLocalStorage();



var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
