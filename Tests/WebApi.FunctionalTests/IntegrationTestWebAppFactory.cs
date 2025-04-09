using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.MsSql;


namespace WebApi.FunctionalTests;

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

            services
                .AddAuthentication("IntegrationTest")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    "IntegrationTest",
                    options => { }

                );

            //services.AddAuthorization(options =>
            //{
            //    options.AddPolicy("RequireAuthenticatedUser", policy =>
            //    {
            //        policy.RequireAuthenticatedUser();
            //        policy.RequireClaim(JwtRegisteredClaimNames.Aud, "ExpectedAudience"); // Ensure audience validation
            //    });
            //});

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
}
