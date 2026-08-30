using Domain.Enums;
using Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using WebApiAuth.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace WebApiAuth.FunctionalTests;


public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _databaseIdContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17")
        .Build();


    private readonly PostgreSqlContainer _databaseContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17")
        .Build();

    /// <summary>
    /// The in-memory realm the host under test is wired to. Singleton so state set up in a test is
    /// visible to the request handling it.
    /// </summary>
    public FakeKeycloakClient Keycloak { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // WebApiAuth resolves these when it builds the typed Keycloak client. The fake replaces that
        // client, but the options are still bound, so give them plausible values.
        builder.UseSetting("Keycloak:BaseUrl", "http://localhost:8080");
        builder.UseSetting("Keycloak:Realm", "taskmaster");
        builder.UseSetting("Keycloak:Authority", "http://localhost:8080/realms/taskmaster");
        builder.UseSetting("Keycloak:Audience", "taskmaster-api");
        builder.UseSetting("Keycloak:ClientId", "taskmaster-auth");
        builder.UseSetting("Keycloak:ClientSecret", "taskmaster-dev-client-secret");

        builder.ConfigureTestServices(async services =>
        {
            // No live Keycloak in the test host — swap the HTTP client for the in-memory realm.
            services.RemoveAll(typeof(IKeycloakClient));
            services.AddSingleton<IKeycloakClient>(Keycloak);

            services.RemoveAll(typeof(DbContextOptions<WebIdentityContext>));

            services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));

            services.AddDbContext<WebIdentityContext>(options =>
                     options.UseNpgsql(_databaseIdContainer.GetConnectionString()));

            services.AddDbContext<ApplicationDbContext>(options =>
                     options.UseNpgsql(_databaseContainer.GetConnectionString()));


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


    /// <summary>
    /// A seeded account, as the tests need it: an id to link a domain User row to (this is the
    /// Keycloak <c>sub</c> now, not an ASP.NET Identity key) and the email to sign in with.
    /// </summary>
    public sealed record SeededUser(string Id, string Email, string UserName);


    public async Task<SeededUser> GetAspNetUserByUserNameAsync(string userName)
    {
        var user = await Keycloak.FindUserByEmailAsync(userName);

        if (user.IsFailure)
            throw new InvalidOperationException($"No seeded Keycloak user '{userName}'.");

        return new SeededUser(user.Value.Id, user.Value.Email!, user.Value.Username);
    }


    public Task<SeededUser> GetAdminUser()
    {
        return GetAspNetUserByUserNameAsync(FakeKeycloakClient.AdminUserEmail);
    }



}
