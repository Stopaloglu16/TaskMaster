using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SharedTestDataLibrary.TaskDataSample;
using Testcontainers.PostgreSql;

namespace WebApi.FunctionalTests;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _databaseContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // WebApi reads ConnectionStrings:taskmasterdb straight out of configuration to build the
        // TickerQ context, and migrates it during startup. Outside Aspire nothing injects that value,
        // so without this every test fails with "The ConnectionString property has not been
        // initialized." before the host even finishes starting.
        builder.UseSetting("ConnectionStrings:taskmasterdb", _databaseContainer.GetConnectionString());

        // AddDefaultAuthentication reads these with GetRequiredValue, which throws on a missing key —
        // without them the host fails to build and every test in the suite errors before it runs.
        // The values are never dialled: TestAuthHandler replaces the JwtBearer scheme below, so no
        // OIDC discovery against this authority ever happens.
        builder.UseSetting("Keycloak:Authority", "http://localhost:8080/realms/taskmaster");
        builder.UseSetting("Keycloak:Audience", "taskmaster-api");

        builder.ConfigureTestServices(services =>
        {

            // ApplicationDbContext
            services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));


            services.AddDbContext<ApplicationDbContext>(options =>
                     options.UseNpgsql(_databaseContainer.GetConnectionString()));


            var sp1 = services.BuildServiceProvider();
            using (var scope = sp1.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<ApplicationDbContext>();
                db.Database.EnsureCreated();

                // Seed test data if needed
                SeedTestData(db);
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

    // seed database with test data
    private void SeedTestData(ApplicationDbContext db)
    {
        var priorityList = TaskPriorityData.GetTaskPriotiryList();

        // Add your test data seeding logic here
        db.TaskPriority.AddRange(priorityList);
        db.SaveChanges();
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
