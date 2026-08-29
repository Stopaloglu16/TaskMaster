using Application.Common.Interfaces;
using Application.Repositories;
using Infrastructure.Data;
using Infrastructure.Messaging;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using ServiceLayer.FileJobs;
using ServiceLayer.FileJobs.Saga;
using ServiceLayer.Users;
using WorkerServiceProcess;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

// Audit stamping needs a user. There is no HttpContext out here, so attribute the saga's writes to
// the system rather than leaving ICurrentUserService unregistered — which is what silently skipped
// audit stamping on every worker write before.
builder.Services.AddSingleton<ICurrentUserService, SystemUserService>();

// DisableRetry is required, not a preference: the messaging layer opens explicit transactions, which
// Npgsql's retrying execution strategy forbids. Reliability comes from the at-least-once outbox
// relay plus inbox dedupe instead.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("taskmasterdb")));
builder.EnrichNpgsqlDbContext<ApplicationDbContext>(settings => settings.DisableRetry = true);

var transport = builder.Configuration.GetMessageTransport();
if (transport is MessageTransport.RabbitMq)
{
    builder.AddRabbitMQClient("rabbitmq");
}

// Registration order is load-bearing:
//   1. the registries, so the modules have somewhere to declare themselves;
//   2. the modules, each declaring one consumer group and its handlers;
//   3. the transport, whose broker topology is derived from those consumer groups.
builder.Services.AddMessagingCore<ApplicationDbContext>();

builder.Services.AddFileJobSagaModule();
builder.Services.AddFileJobWorkerModule();

builder.Services.AddMessageTransport<ApplicationDbContext>(builder.Configuration);

// Repositories/services still used alongside the saga handlers.
builder.Services.AddScoped<IFileJobService, FileJobsService>();
builder.Services.AddScoped<IFileJobRepository, FileJobRepository>();
builder.Services.AddScoped<IFileJobUploadRepository, FileJobUploadRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

var host = builder.Build();

host.Services.GetRequiredService<ILogger<Program>>()
    .LogInformation("FileJob saga worker starting with the {Transport} transport.", transport);

host.Run();
