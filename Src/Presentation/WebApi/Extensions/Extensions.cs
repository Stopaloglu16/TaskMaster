using Application.Common.Interfaces;
using Application.Repositories;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using ServiceLayer.TaskItems;
using ServiceLayer.TaskLists;
using TaskMaster.ServiceDefaults;

namespace WebApi.Extensions;

internal static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        // Add the authentication services to DI
        builder.AddDefaultAuthentication();


        var connectionString = builder.Configuration.GetConnectionString("SqlServerConnection");
        services.AddSqlServer<ApplicationDbContext>(connectionString);

        builder.Services.AddScoped(typeof(IApplicationDbContext), typeof(ApplicationDbContext));

        // Add the integration services that consume the DbContext
        services.AddTransient<ITaskListRepository, TaskListRepository>();
        services.AddTransient<ITaskListService, TaskListService>();

        services.AddTransient<ITaskItemRepository, TaskItemRepository>();
        services.AddTransient<ITaskItemService, TaskItemService>();


        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();


    }

}
