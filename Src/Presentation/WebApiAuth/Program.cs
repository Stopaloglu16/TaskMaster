using Application.Common.Interfaces;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApiAuth.Config;
using WebApiAuth.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
     options.UseSqlServer(connectionString,
    x => x.MigrationsAssembly(@"Infrastructure")
));


builder.Services.AddDbContext<WebIdentityContext>(options =>
     options.UseSqlServer(connectionString,
    x => x.MigrationsAssembly(@"Infrastructure")
));

builder.Services.AddScoped(typeof(IApplicationDbContext), typeof(ApplicationDbContext));


builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<WebIdentityContext>();

builder.Services.AddScoped(typeof(IRepository<,>), typeof(EfCoreRepository<,>));

builder.Services.AddUserServices();


var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();


//JWT token config
builder.Services.AddJtwToken(jwtSettings);

builder.Services.AddHealthChecks();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Version Configuration
builder.Services.AddApiVersioning();



builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

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
