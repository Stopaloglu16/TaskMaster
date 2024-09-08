using Application.Aggregates.UserAuthAggregate;
using SharedUtilityTestMethods;
using System.Net.Http.Json;

namespace WebApiAuth.FunctionalTests.ApiEndPoints;

public class LoginTests : IClassFixture<TestWebApplicationFactory<Program>>, IClassFixture<ApiVersionFixture>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _httpClient;
    private readonly ApiVersionFixture _fixture;

    public LoginTests(TestWebApplicationFactory<Program> factory, ApiVersionFixture fixture)
    {
        _factory = factory;
        _httpClient = factory.CreateClient();
        _factory.RunApiUserMigrations();
        _fixture = fixture;
    }


    [Theory]
    [InlineData("taskmaster@hotmail.co.uk", "SuperStrongPassword+123")]
    public async Task PostAdminLogin_ValidValues_LoginSuccess(string userName, string password)
    {
        //Arrange
        LoginRequest loginRequest = new() { Username = userName, Password = password };

        //Act
        var responseApiLogin = await _httpClient.PostAsJsonAsync($"/api/{_fixture.ApiVersion}/Login/login", loginRequest);

        //Assert
        Assert.True(System.Net.HttpStatusCode.OK == responseApiLogin.StatusCode, $"Login API {responseApiLogin.StatusCode}");

        var apiLoginResponse = await responseApiLogin.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(apiLoginResponse?.RefreshToken);
    }


    [Theory]
    [InlineData("taskmaster@hotmail.co.uk", "SuperWrongPassword+123")]
    public async Task PostAdminLogin_InValidValues_LoginFail(string userName, string password)
    {
        //Arrange
        LoginRequest loginRequest = new() { Username = userName, Password = password };

        //Act
        var responseApiLogin = await _httpClient.PostAsJsonAsync($"/api/{_fixture.ApiVersion}/Login/login", loginRequest);

        //Assert
        Assert.True(System.Net.HttpStatusCode.Unauthorized == responseApiLogin.StatusCode, $"Login API {responseApiLogin.StatusCode}");

    }



}