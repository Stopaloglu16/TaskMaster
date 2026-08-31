using Application.Common.Models;
using System.Text.Json.Serialization;

namespace WebApiAuth.Services;

/// <summary>
/// The token/refresh pair Keycloak returns from the OIDC token endpoint.
/// </summary>
public sealed record KeycloakTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; } = string.Empty;

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; init; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; init; }
}

/// <summary>
/// The outcome of a create attempt. <paramref name="AlreadyExisted"/> is the important part: a
/// realm that already holds the account is not a failure to register, it is a registration to
/// repair, and the caller cannot tell the two apart from an error string.
/// </summary>
/// <param name="Id">The Keycloak id, or null when the user already existed and was not looked up.</param>
public sealed record KeycloakCreateResult(string? Id, bool AlreadyExisted);

/// <summary>A user as the Keycloak Admin REST API represents it (only the fields we use).</summary>
public sealed record KeycloakUser
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("username")]
    public string Username { get; init; } = string.Empty;

    [JsonPropertyName("email")]
    public string? Email { get; init; }
}

/// <summary>
/// WebApiAuth's seam onto Keycloak. Login and refresh go through the client's direct-access grant;
/// user administration goes through the Admin REST API with a service-account token.
/// </summary>
public interface IKeycloakClient
{
    Task<CustomResult<KeycloakTokenResponse>> PasswordGrantAsync(string username, string password, CancellationToken cancellationToken = default);

    Task<CustomResult<KeycloakTokenResponse>> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates the user and returns the new Keycloak id, which becomes the token's <c>sub</c>.
    /// <paramref name="firstName"/> and <paramref name="lastName"/> are not optional in practice:
    /// the realm's user profile requires them, and a user missing either is treated as incomplete —
    /// the direct-access grant then fails with "Account is not fully set up", which surfaces as an
    /// ordinary wrong-password error.
    /// </summary>
    Task<CustomResult<KeycloakCreateResult>> CreateUserAsync(string username, string email, string firstName, string lastName, string password, CancellationToken cancellationToken = default);

    Task<CustomResult> AssignRealmRoleAsync(string userId, string roleName, CancellationToken cancellationToken = default);

    /// <summary>Backfills the required name fields on an account that already exists.</summary>
    Task<CustomResult> UpdateUserProfileAsync(string userId, string firstName, string lastName, CancellationToken cancellationToken = default);

    /// <summary>Removes the account outright, so a deleted user can no longer authenticate.</summary>
    Task<CustomResult> DeleteUserAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Looks a user up by email, falling back to username. The sign-in field is labelled "Username"
    /// and users type either, so an email-only lookup reports real accounts as not registered.
    /// </summary>
    Task<CustomResult<KeycloakUser>> FindUserAsync(string emailOrUsername, CancellationToken cancellationToken = default);

    /// <summary>Sets a new password directly, so the reset page can live in this app rather than Keycloak's.</summary>
    Task<CustomResult> ResetPasswordAsync(string userId, string newPassword, CancellationToken cancellationToken = default);
}
