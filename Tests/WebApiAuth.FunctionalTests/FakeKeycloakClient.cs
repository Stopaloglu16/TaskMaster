using Application.Common.Models;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApiAuth.Services;

namespace WebApiAuth.FunctionalTests;

/// <summary>
/// A seeded, in-memory stand-in for the Keycloak realm, so these tests do not need a live Keycloak
/// container. It mints real (if locally signed) JWTs, because the code under test reads the
/// <c>sub</c> and <c>preferred_username</c> claims back out of the access token.
/// </summary>
public sealed class FakeKeycloakClient : IKeycloakClient
{
    /// <summary>Matches the seeded realm user in TaskMaster.AppHost/Keycloak/taskmaster-realm.json.</summary>
    public const string AdminUserId = "11111111-1111-1111-1111-111111111111";
    public const string AdminUserEmail = "AdminUser@hotmail.co.uk";
    public const string AdminUserPassword = "SuperStrongPassword+123";

    private sealed record FakeUser(string Id, string Username, string Email, string Password);

    private readonly ConcurrentDictionary<string, FakeUser> _users = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Ids whose password has been set through the reset endpoint, for assertions.</summary>
    public ConcurrentBag<string> PasswordsResetFor { get; } = new();

    /// <summary>Ids removed from the realm, for assertions.</summary>
    public ConcurrentBag<string> DeletedUserIds { get; } = new();

    public FakeKeycloakClient()
    {
        // LoginRequestSamples logs in with the email as the username, so both map to the same user.
        _users[AdminUserEmail] = new FakeUser(AdminUserId, AdminUserEmail, AdminUserEmail, AdminUserPassword);
    }


    public Task<CustomResult<KeycloakTokenResponse>> PasswordGrantAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        if (!_users.TryGetValue(username, out var user) || user.Password != password)
            return Task.FromResult(CustomResult<KeycloakTokenResponse>.Failure(CustomError.Failure("Username or password not correct")));

        return Task.FromResult(CustomResult<KeycloakTokenResponse>.Success(IssueTokens(user)));
    }


    public Task<CustomResult<KeycloakTokenResponse>> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        // The fake refresh token is "refresh:<username>".
        var username = refreshToken?.Split(':', 2).ElementAtOrDefault(1);

        if (username is null || !_users.TryGetValue(username, out var user))
            return Task.FromResult(CustomResult<KeycloakTokenResponse>.Failure(CustomError.Failure("Invalid refresh token")));

        return Task.FromResult(CustomResult<KeycloakTokenResponse>.Success(IssueTokens(user)));
    }


    public Task<CustomResult<KeycloakCreateResult>> CreateUserAsync(string username, string email, string firstName, string lastName, string password, CancellationToken cancellationToken = default)
    {
        // Matches the real 409: a conflict is reported as success-with-AlreadyExisted so the caller
        // can repair the account rather than dead-end on it.
        if (_users.ContainsKey(username))
            return Task.FromResult(CustomResult<KeycloakCreateResult>.Success(new KeycloakCreateResult(null, AlreadyExisted: true)));

        // The real realm rejects a user with no first or last name at sign-in time, not at creation,
        // so hold the same line here rather than letting a test pass on a user Keycloak would refuse.
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            return Task.FromResult(CustomResult<KeycloakCreateResult>.Failure(CustomError.Failure("Could not create the user")));

        var id = Guid.NewGuid().ToString();
        _users[username] = new FakeUser(id, username, email, password);

        return Task.FromResult(CustomResult<KeycloakCreateResult>.Success(new KeycloakCreateResult(id, AlreadyExisted: false)));
    }


    public Task<CustomResult> UpdateUserProfileAsync(string userId, string firstName, string lastName, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_users.Values.Any(u => u.Id == userId)
            ? CustomResult.Success()
            : CustomResult.Failure("User not found"));
    }


    public Task<CustomResult> DeleteUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = _users.Values.FirstOrDefault(u => u.Id == userId);

        // The real client treats an already-absent user as done rather than as a failure.
        if (user is null)
            return Task.FromResult(CustomResult.Success());

        _users.TryRemove(user.Username, out _);
        DeletedUserIds.Add(userId);

        return Task.FromResult(CustomResult.Success());
    }


    public Task<CustomResult> AssignRealmRoleAsync(string userId, string roleName, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(CustomResult.Success());
    }


    public Task<CustomResult<KeycloakUser>> FindUserAsync(string emailOrUsername, CancellationToken cancellationToken = default)
    {
        var user = _users.Values.FirstOrDefault(u => string.Equals(u.Email, emailOrUsername, StringComparison.OrdinalIgnoreCase))
                   ?? _users.Values.FirstOrDefault(u => string.Equals(u.Username, emailOrUsername, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(user is null
            ? CustomResult<KeycloakUser>.Failure(CustomError.Failure("User not found"))
            : CustomResult<KeycloakUser>.Success(new KeycloakUser { Id = user.Id, Username = user.Username, Email = user.Email }));
    }


    public Task<CustomResult> ResetPasswordAsync(string userId, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = _users.Values.FirstOrDefault(u => u.Id == userId);

        if (user is null)
            return Task.FromResult(CustomResult.Failure("User not found"));

        _users[user.Username] = user with { Password = newPassword };
        PasswordsResetFor.Add(userId);

        return Task.FromResult(CustomResult.Success());
    }


    private static KeycloakTokenResponse IssueTokens(FakeUser user)
    {
        return new KeycloakTokenResponse
        {
            AccessToken = WriteToken(user),
            RefreshToken = $"refresh:{user.Username}",
            ExpiresIn = 300
        };
    }


    private static string WriteToken(FakeUser user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim("preferred_username", user.Username),
            new Claim("roles", Domain.Enums.UserType.AdminUser.ToString())
        };

        // Signature is irrelevant here — nothing in WebApiAuth validates it, and WebApi (which does)
        // is not in this test host.
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("fake-keycloak-signing-key-for-integration-tests-only"));

        var token = new JwtSecurityToken(
            issuer: "http://localhost:8080/realms/taskmaster",
            audience: "taskmaster-api",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
