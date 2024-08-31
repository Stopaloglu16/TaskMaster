using Application.Aggregates.UserAuthAggregate;
using System.Net.Http.Json;

namespace WebApiAuth.FunctionalTests.ApiEndPoints;

public class LoginTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _httpClient;
    private string _bearerToken;

    public LoginTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient();
        _factory.RunApiUserMigrations();
        _bearerToken = "";
    }


    [Theory]
    [InlineData("taskmaster@hotmail.co.uk", "taskmaster@hotmail.co.uk", "SuperStrongPassword+123")]
    public async Task PostAdminLogin_ValidValues_LoginSuccess(string userName, string email, string password)
    {

        //using (var scope = _factory.Services.CreateScope())
        //{
        //    var db = scope.ServiceProvider.GetService<ApplicationDbContext>();

        //    var myuu = db.Users.ToList();
        //    await db.SaveChangesAsync();
        //}


        LoginRequest loginRequest = new() { Username = userName, Password = password };

        var responseApiLogin = await _httpClient.PostAsJsonAsync("/api/v1/Login/login", loginRequest);


        Assert.True(System.Net.HttpStatusCode.OK == responseApiLogin.StatusCode, $"Login API {responseApiLogin.StatusCode}");

        var apiLoginResponse = await responseApiLogin.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(apiLoginResponse.RefreshToken);
    }
}