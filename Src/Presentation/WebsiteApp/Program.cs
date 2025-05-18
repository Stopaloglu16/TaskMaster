using Blazored.LocalStorage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Radzen;
using TaskMasterRazorClassLibrary.Services;
using WebsiteApp.Components;
using WebsiteApp.Config;
using WebsiteApp.Data;
using WebsiteApp.Handlers;
using WebsiteApp.Services;

var builder = WebApplication.CreateBuilder(args);


builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var appSettingSection = builder.Configuration.GetSection("AppSettings");
builder.Services.Configure<ApiSettingConfig>(appSettingSection);

builder.Services.AddTransient<ValidateHeaderHandler>();

builder.Services.AddAuthorizationCore();
//builder.Services.AddAuthentication(); // Registers IAuthenticationService


//TODO update audienceee!
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(jwtOptions =>
{
    jwtOptions.Authority = builder.Configuration["AppSettings:ApiAuthUrl"];
    jwtOptions.Audience = builder.Configuration["AppSettings:ApiAuthUrl"];
});


builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

// Register WebApiService with two HttpClient instances
builder.Services.AddHttpClient("DefaultClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["AppSettings:ApiUrl"]);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient("AuthClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["AppSettings:ApiAuthUrl"]);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});


builder.Services.AddScoped(typeof(IWebApiService<,>), typeof(WebApiService<,>));

builder.Services.AddScoped<IAuthService, AuthService>(); // Authentication service

builder.Services.AddScoped<IToastService, ToastService>();

builder.Services.AddBlazorServices();

builder.Services.AddBlazoredLocalStorage();


builder.Services.AddRadzenComponents();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


//app.MapBlazorHub();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
