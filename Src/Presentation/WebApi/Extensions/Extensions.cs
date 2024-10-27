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

        // Pooling is disabled because of the following error:
        // Unhandled exception. System.InvalidOperationException:
        // The DbContext of type 'OrderingContext' cannot be pooled because it does not have a public constructor accepting a single parameter of type DbContextOptions or has more than one constructor.
        //services.AddDbContext<ApplicationDbContext>(options =>
        //{
        //    options.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnection"));
        //});

        //builder.Services.AddSqlite<ApplicationDbContext>();

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
