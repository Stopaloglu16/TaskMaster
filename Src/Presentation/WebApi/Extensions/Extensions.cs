using Application.Common.Interfaces;
using Application.Repositories;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using ServiceLayer.Dashboards;
using ServiceLayer.FileJobs;
using ServiceLayer.TaskItems;
using ServiceLayer.TaskLists;
using ServiceLayer.TaskPriorities;
using ServiceLayer.Users;
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
        services.AddScoped<ITaskListRepository, TaskListRepository>();
        services.AddScoped<ITaskListService, TaskListService>();

        services.AddScoped<ITaskItemRepository, TaskItemRepository>();
        services.AddScoped<ITaskItemService, TaskItemService>();

        services.AddScoped<ITaskPriorityRepository, TaskPriorityRepository>();
        services.AddScoped<ITaskPriorityService, TaskPriorityService>();

        services.AddScoped<IDashboardRepository, DashboardRepository>();
        services.AddScoped<IDashboardService, DashboardService>();

        services.AddScoped<IFileJobRepository, FileJobRepository>();
        services.AddScoped<IFileJobService, FileJobsService>();

        services.AddScoped<IFileJobUploadRepository, FileJobUploadRepository>();    
        //services.AddScoped<IFileJobUploadService, FileJobUploadService>();


        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();
        
        services.AddHttpContextAccessor();
    }

}
