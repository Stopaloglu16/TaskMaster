using Application.Common.Interfaces;
using Application.Repositories;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Infrastructure.Repositories.SearchRepos;
using Microsoft.EntityFrameworkCore;
using ServiceLayer.AdvancedSearches;
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


        // ApplicationDbContext takes ICurrentUserService as well as its options, so it cannot be
        // pooled — register it by hand and Enrich* for the Aspire health checks/tracing/retries.
        var connectionString = builder.Configuration.GetConnectionString("taskmasterdb");
        // DisableRetry: the messaging layer opens explicit transactions, which Npgsql's retrying
        // execution strategy forbids.
        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
        builder.EnrichNpgsqlDbContext<ApplicationDbContext>(settings => settings.DisableRetry = true);

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
        
        services.AddScoped<IAdvancedSearchRepository, AdvancedSearchRepository>();
        services.AddScoped<IAdvancedSearchService, AdvancedSearchService>();


        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();
        
        services.AddHttpContextAccessor();
    }

}
