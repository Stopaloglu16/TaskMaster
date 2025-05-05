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

public class UsersTests : BaseIntegrationTest
{
    private string? _bearerToken;

    public UsersTests(IntegrationTestWebAppFactory factory, ApiVersionFixture fixture) : base(factory, fixture)
    {
        _bearerToken = "";
    }

    [Fact]
    public async Task CreateAdminUser_ValidValues_ReturnSuccess()
    {
        //Arrange
        #region LogIn

        var identityUserMock = await _factory.GetAdminUser();

        var userMock = await _dbContext.Users.FirstAsync();

        userMock.AspId = identityUserMock.Id;
        _dbContext.Users.Update(userMock);

        await _dbContext.SaveChangesAsync();

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


        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, createUserResponse.StatusCode);


        FetchInboxRequest request1 = new FetchInboxRequest() { Domain = MailinatorDomain, Inbox = MailinatorDomain };
        var mailResponse1 = await mailinatorClient.MessagesClient.FetchInboxAsync(request1);

        string mailId = string.Empty;

        foreach (var message in mailResponse1.Messages)
        {
            if (message.Subject == "Register")
            {
                mailId = message.Id;
                var request = new FetchMessageRequest() { Domain = MailinatorDomain, MessageId = mailId };
                var responseFetch = await mailinatorClient.MessagesClient.FetchMessageAsync(request);

                var textArray = responseFetch.Text.Split('|');

                if (createUserRequest.FullName == textArray[0].ToString())
                {
                    Assert.True(textArray[0] == createUserRequest.UserEmail, "User email not same");
                    Assert.True(responseFetch.Subject.Contains("Welcome"), "Email not sent");
                    break;
                }
            }
        }

        await mailinatorClient.MessagesClient.DeleteMessageAsync(new DeleteMessageRequest() { Domain = MailinatorDomain, Inbox = MailinatorDomain, MessageId = mailId });

    }
}