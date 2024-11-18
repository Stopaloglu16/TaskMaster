using Application.Aggregates.UserAuthAggregate;
using Microsoft.EntityFrameworkCore;
using SharedTestDataLibrary.UserDataSample;
using SharedUtilityTestMethods;
using System.Net.Http.Json;

namespace WebApiAuth.FunctionalTests.ApiEndPoints;

public class LoginTests : BaseIntegrationTest
{

    public LoginTests(IntegrationTestWebAppFactory factory, ApiVersionFixture fixture) : base(factory, fixture)
    {

    }

    [Fact]
    public async Task Create_ShouldCreateUser()
    {

        var identityUserMock = await _factory.RegisterUser();

        var userMock = await _dbContext.Users.FirstAsync();

        userMock.AspId = identityUserMock.Id;
        _dbContext.Users.Update(userMock);

        await _dbContext.SaveChangesAsync();


        //Arrange
        var loginRequest = LoginRequestSamples.CreateLoginRequestValidSample();

        //Act
        var responseApiLogin = await _httpClient.PostAsJsonAsync($"/api/v1.0/Login/login", loginRequest);


        //Assert
        Assert.True(System.Net.HttpStatusCode.OK == responseApiLogin.StatusCode, $"Login API {responseApiLogin.StatusCode}");

        var apiLoginResponse = await responseApiLogin.Content.ReadFromJsonAsync<UserLoginResponse>();

        Assert.NotNull(apiLoginResponse?.RefreshToken);
    }

}
