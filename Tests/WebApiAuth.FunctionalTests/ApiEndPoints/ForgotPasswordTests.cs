using Application.Aggregates.UserAuthAggregate;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SharedUtilityTestMethods;
using System.Net.Http.Json;
using System.Text;
using ForgotPasswordRequest = Application.Aggregates.UserAuthAggregate.ForgotPasswordRequest;

namespace WebApiAuth.FunctionalTests.ApiEndPoints;

public class ForgotPasswordTests : BaseIntegrationTest
{

    public ForgotPasswordTests(IntegrationTestWebAppFactory factory, ApiVersionFixture fixture) : base(factory, fixture)
    {

    }

    /// <summary>
    /// The whole reset round trip, now that the set-password page belongs to this app rather than
    /// Keycloak: forgotpassword mints a token and mails a link, resetpassword redeems it, and the
    /// new password works at the login endpoint.
    /// </summary>
    [Fact]
    public async Task Resetpassword_ValidValues_Success()
    {
        //Arrange
        var identityUserMock = await _factory.GetAdminUser();

        var userMock = await _dbContext.Users.FirstAsync();
        userMock.UserEmail = identityUserMock.Email;
        userMock.AspId = identityUserMock.Id;

        _dbContext.Users.Update(userMock);

        await _dbContext.SaveChangesAsync();

        string newPassword = TextGenerator.GeneratePassword(8);

        //Act
        //Step 1 - request the reset
        ForgotPasswordRequest forgotPasswordRequest = new ForgotPasswordRequest()
        {
            Username = identityUserMock.Email
        };

        var json = JsonConvert.SerializeObject(forgotPasswordRequest);
        var content1 = new StringContent(json, Encoding.UTF8, "application/json");

        var forgotResponse = await _httpClient.PostAsync($"/api/{_fixture.ApiVersion}/forgotpassword", content1);

        Assert.True(System.Net.HttpStatusCode.OK == forgotResponse.StatusCode,
            $"forgotpassword API {forgotResponse.StatusCode}: {await forgotResponse.Content.ReadAsStringAsync()}");

        // The token only ever leaves the system in the email, so that is where the test reads it —
        // the same place a real user gets it from.
        var sentEmail = Assert.Single(_factory.Emails.ForgotPasswordEmails);
        Assert.Equal(identityUserMock.Email, sentEmail.To);

        //Step 2 - redeem it
        ResetPasswordRequest resetPasswordRequest = new ResetPasswordRequest()
        {
            Username = identityUserMock.Email,
            TokenConfirm = sentEmail.Token,
            Password = newPassword,
            ConfirmPassword = newPassword
        };

        json = JsonConvert.SerializeObject(resetPasswordRequest);
        content1 = new StringContent(json, Encoding.UTF8, "application/json");

        var resetResponse = await _httpClient.PostAsync($"/api/{_fixture.ApiVersion}/resetpassword", content1);

        Assert.True(System.Net.HttpStatusCode.OK == resetResponse.StatusCode,
            $"resetpassword API {resetResponse.StatusCode}: {await resetResponse.Content.ReadAsStringAsync()}");

        //Assert - the new password is the one that works now
        UserLoginRequest loginRequest = new UserLoginRequest()
        {
            Username = identityUserMock.Email,
            Password = newPassword
        };

        var responseApiLogin = await _httpClient.PostAsJsonAsync($"/api/{_fixture.ApiVersion}/Login/login", loginRequest);

        Assert.True(System.Net.HttpStatusCode.OK == responseApiLogin.StatusCode,
            $"Login API {responseApiLogin.StatusCode}: {await responseApiLogin.Content.ReadAsStringAsync()}");

        // And the link cannot be replayed.
        var replayResponse = await _httpClient.PostAsync($"/api/{_fixture.ApiVersion}/resetpassword",
            new StringContent(json, Encoding.UTF8, "application/json"));

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, replayResponse.StatusCode);
    }
}
