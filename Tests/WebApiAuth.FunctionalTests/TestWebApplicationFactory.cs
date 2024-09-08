using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Data.Common;

namespace WebApiAuth.FunctionalTests;

public class TestWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{

    
    public void RunMigrations()
    {
        using (var scope = this.Services.CreateScope())
        {
            var idb = scope.ServiceProvider.GetService<WebIdentityContext>();
            idb.Database.Migrate();

            var db = scope.ServiceProvider.GetService<ApplicationDbContext>();
            db.Database.Migrate();
        }
    }


    public void RunApiUserMigrations()
    {
        using (var scope = this.Services.CreateScope())
        {
            var idb = scope.ServiceProvider.GetService<WebIdentityContext>();
            idb.Database.Migrate();

            var identityUser = idb.Users.Single();

            var db = scope.ServiceProvider.GetService<ApplicationDbContext>();
            db.Database.Migrate();


            User user = new User()
            {
                AspId = identityUser.Id,
                FullName = "Admin User",
                UserEmail = identityUser.Email,
                UserTypeId = UserType.AdminUser
            };
            db.Users.Add(user);
            db.SaveChanges();
        }
    }



    protected override IHost CreateHost(IHostBuilder builder)
    {
        //builder.UseEnvironment("Development"); // will not send real emails
        //var host = builder.Build();
        //host.Start();

        builder.ConfigureServices(services =>
        {
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType ==
                    typeof(DbContextOptions<ApplicationDbContext>));

            if (dbContextDescriptor != null) services.Remove(dbContextDescriptor);

            // Create open SqliteConnection so EF won't automatically close it.
            services.AddSingleton<DbConnection>(container =>
            {
                var connection = new SqliteConnection("DataSource=:memory:");
                connection.Open();

                return connection;
            });


            services.AddDbContext<ApplicationDbContext>((container, options) =>
            {
                options.UseSqlite(container.GetRequiredService<DbConnection>(),
                                  x => x.MigrationsAssembly("Infrastructure.SqliteMigrations"));
            });

            services.AddScoped<ICurrentUserService, MockCurrentUserService>();



            //Identity DbContext
            var dbContextUser = services.SingleOrDefault(
                d => d.ServiceType ==
                    typeof(DbContextOptions<WebIdentityContext>));

            if (dbContextUser != null) services.Remove(dbContextUser);


            services.AddDbContext<WebIdentityContext>((container, options) =>
            {
                options.UseSqlite(container.GetRequiredService<DbConnection>(),
                                  x => x.MigrationsAssembly("Infrastructure.SqliteMigrations"));
            });

        });

        return base.CreateHost(builder);
    }


}