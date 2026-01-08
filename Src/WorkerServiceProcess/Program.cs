using Application.Repositories;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using ServiceLayer.FileJobs;
using ServiceLayer.Users;
using WorkerServiceProcess;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();


// Configure ApplicationDbContext with a connection string from configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection")));


// Register ServiceLayer services needed by this worker. The ServiceLayer project contains the IFileJobService implementation.
builder.Services.AddScoped<IFileJobService, FileJobsService>();
builder.Services.AddScoped<IFileJobRepository, FileJobRepository>();
builder.Services.AddScoped<IFileJobUploadRepository, FileJobUploadRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();


builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
