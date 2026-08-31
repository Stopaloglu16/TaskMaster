using Application.Aggregates.UserAggregate.Commands;
using Application.Aggregates.UserAuthAggregate;
using mailinator_csharp_client.Models.Messages.Requests;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SharedTestDataLibrary.UserDataSample;
using SharedUtilityTestMethods;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace WebApiAuth.FunctionalTests.ApiEndPoints;

public class RegisterTests : BaseIntegrationTest
{

    private string? _bearerToken;

    public RegisterTests(IntegrationTestWebAppFactory factory, ApiVersionFixture fixture) : base(factory, fixture)
    {
        _bearerToken = "";
    }

    [Fact]
    public async Task Create_RegisterUser_Success()
    {

        var identityUserMock = await _factory.GetAdminUser();

        var userMock = await _dbContext.Users.FirstAsync();

        userMock.AspId = identityUserMock.Id;
        _dbContext.Users.Update(userMock);

        await _dbContext.SaveChangesAsync();



        //Arrange
        #region LogInByAdmin

        UserLoginRequest loginRequest = LoginRequestSamples.CreateLoginRequestValidSample();

        var responseApiLogin = await _httpClient.PostAsJsonAsync($"/api/{_fixture.ApiVersion}/Login/login", loginRequest);
        Assert.True(System.Net.HttpStatusCode.OK == responseApiLogin.StatusCode, $"Login API {responseApiLogin.StatusCode}");

        var apiLoginResponse = await responseApiLogin.Content.ReadFromJsonAsync<UserLoginResponse>();

        _bearerToken = apiLoginResponse?.AccessToken;

        #endregion


        //Act
        CreateUserRequest createUserRequest = UserRequestData.CreateUserRequestValidAdminSample();

        var json = JsonConvert.SerializeObject(createUserRequest);
        var content1 = new StringContent(json, Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _bearerToken);

        var createUserResponse = await _httpClient.PostAsync($"/api/{_fixture.ApiVersion}/Users", content1);


        Assert.True(System.Net.HttpStatusCode.OK == createUserResponse.StatusCode, $"createUser API {createUserResponse.StatusCode}");


        // The invite token reaches the user only in the email, so read it from the captured mail
        // rather than a live Mailinator inbox.
        var inviteEmail = Assert.Single(_factory.Emails.RegisterEmails,
                                        e => e.To == createUserRequest.UserEmail);

        string mockRegisterUserEmail = inviteEmail.Username;
        string mockRegisterUserToken = inviteEmail.Token;
        string mockRegisterUserPassword = TextGenerator.GeneratePassword(8);


        // Assert
        RegisterUserRequest registerUserRequest = new RegisterUserRequest()
        {
            Username = mockRegisterUserEmail,
            TokenConfirm = mockRegisterUserToken,
            Password = mockRegisterUserPassword,
            ConfirmPassword = mockRegisterUserPassword
        };



        var json2 = JsonConvert.SerializeObject(registerUserRequest);
        var content2 = new StringContent(json2, Encoding.UTF8, "application/json");
        var responseRegisterUser = await _httpClient.PostAsync($"/api/{_fixture.ApiVersion}/registerusers", content2);

        Assert.True(System.Net.HttpStatusCode.OK == responseRegisterUser.StatusCode,
            $"registerusers API {responseRegisterUser.StatusCode}: {await responseRegisterUser.Content.ReadAsStringAsync()}");

        // Registering has to leave an account that can actually sign in. It did not before: the
        // Keycloak user was created without a first or last name, and the direct-access grant then
        // refused it with "Account is not fully set up".
        UserLoginRequest newUserLogin = new UserLoginRequest()
        {
            Username = mockRegisterUserEmail,
            Password = mockRegisterUserPassword
        };

        _httpClient.DefaultRequestHeaders.Authorization = null;

        var newUserLoginResponse = await _httpClient.PostAsJsonAsync($"/api/{_fixture.ApiVersion}/Login/login", newUserLogin);

        Assert.True(System.Net.HttpStatusCode.OK == newUserLoginResponse.StatusCode,
            $"Login API {newUserLoginResponse.StatusCode}: {await newUserLoginResponse.Content.ReadAsStringAsync()}");
    }

}
