using Domain.Enums;
using Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.MsSql;

namespace WebApiAuth.FunctionalTests;


public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _databaseIdContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("Strong_password_123!")
        .Build();


    private readonly MsSqlContainer _databaseContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("Strong_password_123!")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(async services =>
        {


            services.RemoveAll(typeof(DbContextOptions<WebIdentityContext>));

            services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));

            services.AddDbContext<WebIdentityContext>(options =>
                     options.UseSqlServer(_databaseIdContainer.GetConnectionString()));

            services.AddDbContext<ApplicationDbContext>(options =>
                     options.UseSqlServer(_databaseContainer.GetConnectionString()));


            var sp = services.BuildServiceProvider();
            using (var scope = sp.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<WebIdentityContext>();
                db.Database.EnsureCreated();

                var db1 = scopedServices.GetRequiredService<ApplicationDbContext>();

                db1.Database.EnsureCreated();


                // Seed test data if needed
                //SeedTestData(db);
            }

        });


    }

    public async Task InitializeAsync()
    {
        await _databaseContainer.StartAsync();
        await _databaseIdContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _databaseContainer.StopAsync();
        await _databaseContainer.DisposeAsync();

        await _databaseIdContainer.StopAsync();
        await _databaseIdContainer.DisposeAsync();
    }


    public async Task<IdentityUser> GetAspNetUserByUserNameAsync(string userName)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WebIdentityContext>();

        return await context.Users.Where(uu => uu.UserName == userName).FirstAsync();
    }


    public async Task<IdentityUser> RegisterUser()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WebIdentityContext>();

        return await context.Users.Where(uu => uu.UserName == UserType.AdminUser.ToString()).FirstAsync();
    }



}
