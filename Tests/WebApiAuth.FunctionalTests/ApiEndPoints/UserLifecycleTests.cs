using Application.Aggregates.UserAggregate.Commands;
using Application.Aggregates.UserAuthAggregate;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SharedUtilityTestMethods;
using System.Net.Http.Json;
using System.Text;

namespace WebApiAuth.FunctionalTests.ApiEndPoints;

/// <summary>
/// The two paths that were dead ends: repairing a half-registered account, and removing a user.
/// </summary>
public class UserLifecycleTests : BaseIntegrationTest
{
    public UserLifecycleTests(IntegrationTestWebAppFactory factory, ApiVersionFixture fixture) : base(factory, fixture)
    {
    }


    /// <summary>
    /// Registering a second time used to 409 forever, stranding any account whose first attempt
    /// half-succeeded. It now adopts the existing Keycloak user and finishes the job.
    /// </summary>
    [Fact]
    public async Task Register_WhenKeycloakUserAlreadyExists_RepairsAndAllowsLogin()
    {
        //Arrange — invite a user and read the token out of the invite mail
        var email = $"repair{Guid.NewGuid():N}@example.com";

        var createUserRequest = new CreateUserRequest
        {
            FullName = "Repair Me",
            UserEmail = email,
            UserType = UserType.TaskUser
        };

        var createResponse = await _httpClient.PostAsJsonAsync($"/api/{_fixture.ApiVersion}/Users", createUserRequest);
        Assert.Equal(System.Net.HttpStatusCode.OK, createResponse.StatusCode);

        var invite = Assert.Single(_factory.Emails.RegisterEmails, e => e.To == email);

        // Stand the Keycloak account up behind the app's back, exactly as a half-finished first
        // attempt would leave it: the realm knows the user, the domain row still has no AspId.
        var preExisting = await _factory.Keycloak.CreateUserAsync(email, email, "Stale", "Name", "OldPassword+1");
        Assert.True(preExisting.IsSuccess);

        string newPassword = TextGenerator.GeneratePassword(8);

        //Act — register over the top of it
        var registerRequest = new RegisterUserRequest
        {
            Username = email,
            TokenConfirm = invite.Token,
            Password = newPassword,
            ConfirmPassword = newPassword
        };

        var json = JsonConvert.SerializeObject(registerRequest);
        var registerResponse = await _httpClient.PostAsync($"/api/{_fixture.ApiVersion}/registerusers",
                                                           new StringContent(json, Encoding.UTF8, "application/json"));

        //Assert
        Assert.True(System.Net.HttpStatusCode.OK == registerResponse.StatusCode,
            $"registerusers API {registerResponse.StatusCode}: {await registerResponse.Content.ReadAsStringAsync()}");

        // The password the user just chose is the one that works, and the domain row is linked.
        var loginResponse = await _httpClient.PostAsJsonAsync($"/api/{_fixture.ApiVersion}/Login/login",
            new UserLoginRequest { Username = email, Password = newPassword });

        Assert.True(System.Net.HttpStatusCode.OK == loginResponse.StatusCode,
            $"Login API {loginResponse.StatusCode}: {await loginResponse.Content.ReadAsStringAsync()}");

        var repairedUser = await _dbContext.Users.AsNoTracking().FirstAsync(u => u.UserEmail == email);
        Assert.False(string.IsNullOrWhiteSpace(repairedUser.AspId));

        // And the invite token is burned, so the link cannot be replayed.
        var replayResponse = await _httpClient.PostAsync($"/api/{_fixture.ApiVersion}/registerusers",
                                                         new StringContent(json, Encoding.UTF8, "application/json"));

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, replayResponse.StatusCode);
    }


    /// <summary>
    /// Deleting from UserManager has to remove the realm account too, or a "deleted" user can still
    /// authenticate.
    /// </summary>
    [Fact]
    public async Task Delete_RemovesKeycloakAccount_AndHidesTheUser()
    {
        //Arrange — a fully registered user
        var email = $"delete{Guid.NewGuid():N}@example.com";

        var createResponse = await _httpClient.PostAsJsonAsync($"/api/{_fixture.ApiVersion}/Users",
            new CreateUserRequest { FullName = "Delete Me", UserEmail = email, UserType = UserType.TaskUser });

        Assert.Equal(System.Net.HttpStatusCode.OK, createResponse.StatusCode);

        var invite = Assert.Single(_factory.Emails.RegisterEmails, e => e.To == email);
        string password = TextGenerator.GeneratePassword(8);

        var registerResponse = await _httpClient.PostAsJsonAsync($"/api/{_fixture.ApiVersion}/registerusers",
            new RegisterUserRequest { Username = email, TokenConfirm = invite.Token, Password = password, ConfirmPassword = password });

        Assert.Equal(System.Net.HttpStatusCode.OK, registerResponse.StatusCode);

        var user = await _dbContext.Users.AsNoTracking().FirstAsync(u => u.UserEmail == email);
        var keycloakId = user.AspId!;

        //Act
        var deleteResponse = await _httpClient.DeleteAsync($"/api/{_fixture.ApiVersion}/Users/user/{user.Id}");

        //Assert
        Assert.True(System.Net.HttpStatusCode.OK == deleteResponse.StatusCode,
            $"delete API {deleteResponse.StatusCode}: {await deleteResponse.Content.ReadAsStringAsync()}");

        Assert.Contains(keycloakId, _factory.Keycloak.DeletedUserIds);

        // Soft delete, not a hard one: the row is still there, flagged, and out of the grid query.
        var deletedUser = await _dbContext.Users.AsNoTracking()
                                          .IgnoreQueryFilters()
                                          .FirstAsync(u => u.UserEmail == email);

        Assert.Equal(1, deletedUser.IsDeleted);

        // And the account can no longer sign in.
        var loginResponse = await _httpClient.PostAsJsonAsync($"/api/{_fixture.ApiVersion}/Login/login",
            new UserLoginRequest { Username = email, Password = password });

        Assert.NotEqual(System.Net.HttpStatusCode.OK, loginResponse.StatusCode);
    }
}
