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

    /// <summary>Creates the user and returns the new Keycloak id, which becomes the token's <c>sub</c>.</summary>
    Task<CustomResult<string>> CreateUserAsync(string username, string email, string password, CancellationToken cancellationToken = default);

    Task<CustomResult> AssignRealmRoleAsync(string userId, string roleName, CancellationToken cancellationToken = default);

    Task<CustomResult<KeycloakUser>> FindUserByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>Has Keycloak email the user an UPDATE_PASSWORD action link, using the realm's SMTP settings.</summary>
    Task<CustomResult> SendUpdatePasswordEmailAsync(string userId, CancellationToken cancellationToken = default);
}
