using Application.Aggregates.UserAuthAggregate;
using Application.Aggregates.UserAuthAggregate.Token;
using Microsoft.EntityFrameworkCore;
using SharedTestDataLibrary.UserDataSample;
using SharedUtilityTestMethods;
using System.Net.Http.Json;

namespace WebApiAuth.FunctionalTests.ApiEndPoints;

public class RefreshRequestTests : BaseIntegrationTest
{

    public RefreshRequestTests(IntegrationTestWebAppFactory factory, ApiVersionFixture fixture) : base(factory, fixture)
    {

    }

    [Fact]
    public async Task CreateRefrehToken_Valid_Success()
    {
        var identityUserMock = await _factory.GetAdminUser();

        var userMock = await _dbContext.Users.FirstAsync();

        userMock.AspId = identityUserMock.Id;
        _dbContext.Users.Update(userMock);

        await _dbContext.SaveChangesAsync();

        var loginRequest = LoginRequestSamples.CreateLoginRequestValidSample();

        var responseApiLogin = await _httpClient.PostAsJsonAsync($"/api/v1.0/Login/login", loginRequest);

        Assert.True(System.Net.HttpStatusCode.OK == responseApiLogin.StatusCode, $"Login API {responseApiLogin.StatusCode}");

        var apiLoginResponse = await responseApiLogin.Content.ReadFromJsonAsync<UserLoginResponse>();

        Assert.NotNull(apiLoginResponse?.RefreshToken);

        //Arrange
        RefreshTokenRequest refreshTokenRequest = new RefreshTokenRequest()
        {
            RefreshToken = apiLoginResponse?.RefreshToken,
            AccessToken = apiLoginResponse.AccessToken
        };

        await Task.Delay(65000);

        //Act
        var refreshRequestApi = await _httpClient.PostAsJsonAsync($"/api/v1.0/Login/refresh-token", refreshTokenRequest);


        //Assert
        Assert.True(System.Net.HttpStatusCode.OK == refreshRequestApi.StatusCode, $"Refresh request {refreshRequestApi.StatusCode}");

        var returnMessage = refreshRequestApi.Content.ReadAsStringAsync();

        Assert.NotNull(returnMessage);
    }

}
