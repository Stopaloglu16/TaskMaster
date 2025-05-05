using Application.Aggregates.UserAuthAggregate;
using mailinator_csharp_client.Models.Messages.Requests;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SharedUtilityTestMethods;
using System;
using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using ForgotPasswordRequest = Application.Aggregates.UserAuthAggregate.ForgotPasswordRequest;

namespace WebApiAuth.FunctionalTests.ApiEndPoints;


public class ForgotPasswordTests : BaseIntegrationTest
{

    public ForgotPasswordTests(IntegrationTestWebAppFactory factory, ApiVersionFixture fixture) : base(factory, fixture)
    {

    }

    [Fact]
    public async Task Resetpassword_ValidValues_Success()
    {
        //Arrange
        #region LogIn

        var identityUserMock = await _factory.GetAdminUser();

        var userMock = await _dbContext.Users.FirstAsync();
        userMock.UserEmail = identityUserMock.Email;
        userMock.AspId = identityUserMock.Id;

        _dbContext.Users.Update(userMock);

        await _dbContext.SaveChangesAsync();

        #endregion

        //Act
        //Step 1
        //Send Forgot Password API

        //CreateUserRequest createUserRequest = UserData.CreateUserRequestValidAdminSample();

        ForgotPasswordRequest forgotPasswordRequest = new ForgotPasswordRequest()
        {
            Username = identityUserMock.Email
        };

        var json = JsonConvert.SerializeObject(forgotPasswordRequest);
        var content1 = new StringContent(json, Encoding.UTF8, "application/json");

        var createUserResponse = await _httpClient.PostAsync($"/api/{_fixture.ApiVersion}/forgotpassword", content1);

        if (!createUserResponse.IsSuccessStatusCode)
        {
            var wer = await createUserResponse.Content.ReadAsStringAsync();
        }


        Assert.Equal(System.Net.HttpStatusCode.OK, createUserResponse.StatusCode);

        FetchInboxRequest request1 = new FetchInboxRequest() { Domain = MailinatorDomain, Inbox = MailinatorDomain };
        var mailResponse1 = await mailinatorClient.MessagesClient.FetchInboxAsync(request1);


        string mockRegisterUserEmail = string.Empty;
        string mockRegisterUserToken = string.Empty;
        string mockRegisterUserPassword = TextGenerator.GeneratePassword(8);
        string mailId = string.Empty;

        foreach (var message in mailResponse1.Messages)
        {
            if (message.Subject == "Forgot Password")
            {
                mailId = message.Id;
                var request = new FetchMessageRequest() { Domain = MailinatorDomain, MessageId = mailId };
                var responseFetch = await mailinatorClient.MessagesClient.FetchMessageAsync(request);

                string urlPattern = @"<a href='([^']*)'>";
                Match match = Regex.Match(responseFetch.Text.ToString(), urlPattern);

                var textArray = match.Groups[1].Value;

                // Parse the URL
                Uri uri = new Uri(textArray);

                // Extract query parameters
                var queryParams = HttpUtility.ParseQueryString(uri.Query);

                mockRegisterUserEmail = queryParams["username"];
                mockRegisterUserToken = queryParams["token"];


                //if (identityUserMock.Email == textArray[4].ToString())
                //{

                //    mockRegisterUserEmail = textArray[4].ToString();

                //    for (int i = 5; i < textArray.Length; i++)
                //    {
                //        mockRegisterUserToken += textArray[i];

                //        if (i < textArray.Length - 1)
                //        {
                //            mockRegisterUserToken += "/";
                //        }
                //    }

                //    break;
                //}
            }
        }


        //Step 2
        //Reset Password API

        ResetPasswordRequest resetPasswordRequest = new ResetPasswordRequest()
        {
            Email = mockRegisterUserEmail,
            ResetCode = mockRegisterUserToken,
            NewPassword = mockRegisterUserPassword
        };


        json = JsonConvert.SerializeObject(resetPasswordRequest);
        content1 = new StringContent(json, Encoding.UTF8, "application/json");

        createUserResponse = await _httpClient.PostAsync($"/api/{_fixture.ApiVersion}/ResetPassword", content1);

        if (!createUserResponse.IsSuccessStatusCode)
        {
            var wer = await createUserResponse.Content.ReadAsStringAsync();
        }

        Assert.Equal(System.Net.HttpStatusCode.OK, createUserResponse.StatusCode);




        // Assert
        //Check password been reset
        UserLoginRequest loginRequest = new UserLoginRequest()
        {
            Username = mockRegisterUserEmail,
            Password = mockRegisterUserPassword
        };

        var responseApiLogin = await _httpClient.PostAsJsonAsync($"/api/{_fixture.ApiVersion}/Login/login", loginRequest);

        if (!responseApiLogin.IsSuccessStatusCode)
        {
            var wer = await responseApiLogin.Content.ReadAsStringAsync();
        }


        Assert.True(System.Net.HttpStatusCode.OK == responseApiLogin.StatusCode, $"Login API {responseApiLogin.StatusCode}");


        await mailinatorClient.MessagesClient.DeleteMessageAsync(new DeleteMessageRequest() { Domain = MailinatorDomain, Inbox = MailinatorDomain, MessageId = mailId });

    }
}