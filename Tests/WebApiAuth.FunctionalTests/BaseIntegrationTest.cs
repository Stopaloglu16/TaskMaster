using Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using SharedUtilityTestMethods;

namespace WebApiAuth.FunctionalTests;

public abstract class BaseIntegrationTest : EmailTestBase, IClassFixture<IntegrationTestWebAppFactory>, IClassFixture<ApiVersionFixture>
{
    private readonly IServiceScope _scope;
    protected readonly ApplicationDbContext _dbContext;
    protected readonly WebIdentityContext _dbIdContext;
    protected readonly HttpClient _httpClient;
    protected readonly IntegrationTestWebAppFactory _factory;
    protected readonly ApiVersionFixture _fixture;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory, ApiVersionFixture fixture)
    {
        _factory = factory;
        _scope = factory.Services.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        _dbIdContext = _scope.ServiceProvider.GetRequiredService<WebIdentityContext>();
        _httpClient = factory.CreateClient();
        _fixture = fixture;
    }

}
