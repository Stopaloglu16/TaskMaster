using Application.Aggregates.UserAuthAggregate;
using SharedUtilityTestMethods;
using System.Net.Http.Json;
using SharedTestDataLibrary.UserDataSample;

namespace WebApiAuth.FunctionalTests.ApiEndPoints;

public class LoginTests1 : IClassFixture<TestWebApplicationFactory<Program>>, IClassFixture<ApiVersionFixture>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _httpClient;
    private readonly ApiVersionFixture _fixture;

    public LoginTests1(TestWebApplicationFactory<Program> factory, ApiVersionFixture fixture)
    {
        _factory = factory;
        _httpClient = factory.CreateClient();
        _factory.RunApiUserMigrations();
        _fixture = fixture;
    }


    //[Fact]
    //public async Task PostAdminLogin_ValidValues_LoginSuccess()
    //{
    //    //Arrange
    //    var loginRequest = LoginRequestSamples.LoginRequestValidSample();

    //    //Act
    //    var responseApiLogin = await _httpClient.PostAsJsonAsync($"/api/{_fixture.ApiVersion}/Login/login", loginRequest);

    //    //Assert
    //    Assert.True(System.Net.HttpStatusCode.OK == responseApiLogin.StatusCode, $"Login API {responseApiLogin.StatusCode}");

    //    var apiLoginResponse = await responseApiLogin.Content.ReadFromJsonAsync<LoginResponse>();

    //    Assert.NotNull(apiLoginResponse?.RefreshToken);
    //}


    //[Fact]
    //public async Task PostAdminLogin_InValidValues_LoginFail()
    //{
    //    //Arrange
    //    var loginRequest = LoginRequestSamples.LoginRequestValidSample() with { Username = "NotFound" };

    //    //Act
    //    var responseApiLogin = await _httpClient.PostAsJsonAsync($"/api/{_fixture.ApiVersion}/Login/login", loginRequest);

    //    //Assert
    //    Assert.True(System.Net.HttpStatusCode.Unauthorized == responseApiLogin.StatusCode, $"Login API {responseApiLogin.StatusCode}");
    //}


}