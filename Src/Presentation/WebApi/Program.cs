using TaskMaster.ServiceDefaults;
using WebApi.Apis;
using WebApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services DI
builder.AddServiceDefaults();
builder.AddApplicationServices();
builder.Services.AddProblemDetails();
builder.Services.AddAuthorizationBuilder();

var withApiVersioning = builder.Services.AddApiVersioning();
builder.AddDefaultOpenApi(withApiVersioning);



builder.Services.AddHealthChecks();


var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var taskList = app.NewVersionedApi("TaskList");
taskList.TaskListApiV1().RequireAuthorization();


var taskItem = app.NewVersionedApi("TaskItem");
taskItem.TaskItemApiV1().RequireAuthorization();


app.MapHealthChecks("_health");

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.Run();


public partial class Program { }