using Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace WebApi.FunctionalTests;

public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly IServiceScope _scope;
    protected readonly ApplicationDbContext _dbContext;
    protected readonly HttpClient _httpClient;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        _scope = factory.Services.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        _httpClient = factory.CreateClient();
    }

}