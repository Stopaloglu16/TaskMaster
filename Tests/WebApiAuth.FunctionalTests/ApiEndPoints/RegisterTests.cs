﻿using Application.Aggregates.UserAggregate.Commands;
using Application.Aggregates.UserAuthAggregate;
using mailinator_csharp_client.Models.Messages.Requests;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using SharedUtilityTestMethods;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace WebApiAuth.FunctionalTests.ApiEndPoints
{
    public class RegisterTests : EmailTestBase, IClassFixture<TestWebApplicationFactory<Program>>, IClassFixture<ApiVersionFixture>
    {
        private readonly TestWebApplicationFactory<Program> _factory;
        private readonly HttpClient _httpClient;
        private readonly HttpClient _httpClient1;
        private string? _bearerToken;
        private readonly ApiVersionFixture _fixture;

        public RegisterTests(TestWebApplicationFactory<Program> factory, ApiVersionFixture fixture)
        {
            _factory = factory;
            _httpClient = factory.CreateClient();
            _httpClient1 = factory.CreateClient();
            _factory.RunApiUserMigrations();
            _bearerToken = "";
            _fixture = fixture;
        }


        [Theory]
        [InlineData("taskmaster@hotmail.co.uk", "SuperStrongPassword+123")]
        public async Task CreateAdminUser_ValidValues_RegisterSuccess(string userName, string password)
        {
            const string mockUserEmail = "admin1@hotmail.co.uk";


            //Delay for mail reading, or crashing with login
            await Task.Delay(5000);

            //Arrange
            #region LogIn

            LoginRequest loginRequest = new() { Username = userName, Password = password };

            var responseApiLogin = await _httpClient.PostAsJsonAsync($"/api/{_fixture.ApiVersion}/Login/login", loginRequest);
            Assert.True(System.Net.HttpStatusCode.OK == responseApiLogin.StatusCode, $"Login API {responseApiLogin.StatusCode}");

            var apiLoginResponse = await responseApiLogin.Content.ReadFromJsonAsync<LoginResponse>();

            _bearerToken = apiLoginResponse?.AccessToken;

            #endregion

            //Act
            CreateUserRequest createUserRequest = new CreateUserRequest() { FullName = "admin user", UserEmail = mockUserEmail, UserType = Domain.Enums.UserType.AdminUser };

            var json = JsonConvert.SerializeObject(createUserRequest);
            var content1 = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _bearerToken);

            var createUserResponse = await _httpClient.PostAsync($"/api/{_fixture.ApiVersion}/Users", content1);


            Assert.Equal(System.Net.HttpStatusCode.OK, createUserResponse.StatusCode);


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

                if (mockUserEmail == textArray[0].ToString())
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
            var responseRegisterUser = await _httpClient1.PostAsync($"/api/{_fixture.ApiVersion}/registerusers", content2);

            var cc =  await responseRegisterUser.Content.ReadAsStringAsync();

            Assert.Equal(System.Net.HttpStatusCode.OK, responseRegisterUser.StatusCode);

            await mailinatorClient.MessagesClient.DeleteMessageAsync(new DeleteMessageRequest() { Domain = MailinatorDomain, Inbox = MailinatorDomain, MessageId = mailId });
        }

    }
}
