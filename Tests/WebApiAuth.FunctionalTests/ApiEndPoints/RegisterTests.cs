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

        var identityUserMock = await _factory.RegisterUser();

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
        CreateUserRequest createUserRequest = UserData.CreateUserRequestValidAdminSample();

        var json = JsonConvert.SerializeObject(createUserRequest);
        var content1 = new StringContent(json, Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _bearerToken);

        var createUserResponse = await _httpClient.PostAsync($"/api/{_fixture.ApiVersion}/Users", content1);


        Assert.True(System.Net.HttpStatusCode.OK == createUserResponse.StatusCode, $"createUser API {createUserResponse.StatusCode}");


        FetchInboxRequest request1 = new FetchInboxRequest() { Domain = MailinatorDomain, Inbox = MailinatorDomain };
        var mailResponse1 = await mailinatorClient.MessagesClient.FetchInboxAsync(request1);

        string mockRegisterUserEmail = string.Empty;
        string mockRegisterUserToken = string.Empty;
        string mockRegisterUserPassword = TextGenerator.GeneratePassword(8);
        string mailId = string.Empty;

        foreach (var message in mailResponse1.Messages)
        {
            mailId = mailResponse1.Messages[0].Id;
            var request = new FetchMessageRequest() { Domain = MailinatorDomain, MessageId = mailId };
            var responseFetch = await mailinatorClient.MessagesClient.FetchMessageAsync(request);

            var textArray = responseFetch.Text.Split('|');

            if (createUserRequest.UserEmail == textArray[0].ToString())
            {
                mockRegisterUserEmail = textArray[0].ToString();
                mockRegisterUserToken = textArray[1].ToString();

                break;
            }
        }


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

        var cc = await responseRegisterUser.Content.ReadAsStringAsync();

        Assert.True(System.Net.HttpStatusCode.OK == responseRegisterUser.StatusCode, "registerusers API");

        await mailinatorClient.MessagesClient.DeleteMessageAsync(new DeleteMessageRequest() { Domain = MailinatorDomain, Inbox = MailinatorDomain, MessageId = mailId });

    }

}
