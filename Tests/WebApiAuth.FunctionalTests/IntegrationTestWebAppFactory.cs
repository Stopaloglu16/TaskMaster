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
    private readonly MsSqlContainer _databaseContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("Strong_password_123!")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {

            //https://github.com/foadalavi/Test-Foundation-playlist


            // Identity
            services.RemoveAll(typeof(DbContextOptions<WebIdentityContext>));


            services.AddDbContext<WebIdentityContext>(options =>
                     options.UseSqlServer(_databaseContainer.GetConnectionString()));


            var sp = services.BuildServiceProvider();
            using (var scope = sp.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<WebIdentityContext>();
                db.Database.EnsureCreated();

                // Seed test data if needed
                //SeedTestData(db);
            }


            // ApplicationDbContext
            services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));


            services.AddDbContext<ApplicationDbContext>(options =>
                     options.UseSqlServer(_databaseContainer.GetConnectionString()));


            var sp1 = services.BuildServiceProvider();
            using (var scope = sp1.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<ApplicationDbContext>();
                db.Database.EnsureCreated();



                // Seed test data if needed
                //SeedTestData(db);
            }

            //services.AddAuthentication("IntegrationTest")
            //        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
            //            "IntegrationTest",
            //            options => { }
            //        );



        });



      

    }

    public async Task InitializeAsync()
    {
        await _databaseContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _databaseContainer.StopAsync();
    }


    public async Task<IdentityUser> GetAspNetUserByUserNameAsync(string userName)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WebIdentityContext>();

        var context1 = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var uuL =  await context1.Users.ToListAsync();

        return await context.Users.Where(uu => uu.UserName == userName).FirstAsync();
    }

}
