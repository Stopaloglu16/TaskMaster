using Application.Common.Models;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace WebApiAuth.Services;

/// <summary>
/// Talks to Keycloak over HTTP. Everything returns <see cref="CustomResult"/> rather than throwing,
/// matching the convention the rest of the solution follows.
/// </summary>
public sealed class KeycloakClient : IKeycloakClient
{
    private readonly HttpClient _httpClient;
    private readonly KeycloakOptions _options;
    private readonly ILogger<KeycloakClient> _logger;

    // Service-account token cache. Guarded by a semaphore so a burst of registrations does not fire
    // a client_credentials request each.
    private static readonly SemaphoreSlim _adminTokenLock = new(1, 1);
    private static string? _adminToken;
    private static DateTimeOffset _adminTokenExpiresAt = DateTimeOffset.MinValue;

    public KeycloakClient(HttpClient httpClient,
                          IOptions<KeycloakOptions> options,
                          ILogger<KeycloakClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }


    public async Task<CustomResult<KeycloakTokenResponse>> PasswordGrantAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        return await RequestTokenAsync(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["username"] = username,
            ["password"] = password,
            ["scope"] = "openid"
        }, cancellationToken);
    }


    public async Task<CustomResult<KeycloakTokenResponse>> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        return await RequestTokenAsync(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["refresh_token"] = refreshToken
        }, cancellationToken);
    }


    public async Task<CustomResult<KeycloakCreateResult>> CreateUserAsync(string username, string email, string firstName, string lastName, string password, CancellationToken cancellationToken = default)
    {
        var adminToken = await GetAdminTokenAsync(cancellationToken);
        if (adminToken is null)
            return CustomResult<KeycloakCreateResult>.Failure(CustomError.Failure("Could not authenticate to Keycloak"));

        // firstName/lastName are required by the realm's user profile. Leave either out and Keycloak
        // creates the user happily, then refuses every direct-access grant with "Account is not fully
        // set up" — which RequestTokenAsync can only report as a bad password.
        var payload = new
        {
            username,
            email,
            firstName,
            lastName,
            enabled = true,
            emailVerified = true,
            credentials = new[] { new { type = "password", value = password, temporary = false } }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, _options.AdminUsersPath)
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        // Not an error: the caller repairs the existing account rather than giving up on it.
        if (response.StatusCode == HttpStatusCode.Conflict)
            return CustomResult<KeycloakCreateResult>.Success(new KeycloakCreateResult(null, AlreadyExisted: true));

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Keycloak user creation failed ({Status}): {Body}", response.StatusCode, body);
            return CustomResult<KeycloakCreateResult>.Failure(CustomError.Failure("Could not create the user"));
        }

        // Keycloak returns 201 with no body; the new id is the last segment of the Location header.
        var location = response.Headers.Location?.ToString();
        var userId = location?.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();

        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogError("Keycloak user creation returned no Location header.");
            return CustomResult<KeycloakCreateResult>.Failure(CustomError.Failure("Could not create the user"));
        }

        return CustomResult<KeycloakCreateResult>.Success(new KeycloakCreateResult(userId, AlreadyExisted: false));
    }


    public async Task<CustomResult> UpdateUserProfileAsync(string userId, string firstName, string lastName, CancellationToken cancellationToken = default)
    {
        var adminToken = await GetAdminTokenAsync(cancellationToken);
        if (adminToken is null)
            return CustomResult.Failure("Could not authenticate to Keycloak");

        using var request = new HttpRequestMessage(HttpMethod.Put, $"{_options.AdminUsersPath}/{userId}")
        {
            Content = JsonContent.Create(new { firstName, lastName, enabled = true, emailVerified = true })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Updating profile for {UserId} failed ({Status}): {Body}", userId, response.StatusCode, body);
            return CustomResult.Failure("Could not update the user profile");
        }

        return CustomResult.Success();
    }


    public async Task<CustomResult> DeleteUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var adminToken = await GetAdminTokenAsync(cancellationToken);
        if (adminToken is null)
            return CustomResult.Failure("Could not authenticate to Keycloak");

        using var request = new HttpRequestMessage(HttpMethod.Delete, $"{_options.AdminUsersPath}/{userId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        // Already gone is the state we wanted, so treat 404 as done rather than as a failure.
        if (response.StatusCode == HttpStatusCode.NotFound)
            return CustomResult.Success();

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Deleting Keycloak user {UserId} failed ({Status}): {Body}", userId, response.StatusCode, body);
            return CustomResult.Failure("Could not delete the Keycloak account");
        }

        return CustomResult.Success();
    }


    public async Task<CustomResult> AssignRealmRoleAsync(string userId, string roleName, CancellationToken cancellationToken = default)
    {
        var adminToken = await GetAdminTokenAsync(cancellationToken);
        if (adminToken is null)
            return CustomResult.Failure("Could not authenticate to Keycloak");

        // The role mapping has to carry the role's id, so look the role up first.
        using var roleRequest = new HttpRequestMessage(HttpMethod.Get, $"{_options.AdminRealmRolesPath}/{Uri.EscapeDataString(roleName)}");
        roleRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        using var roleResponse = await _httpClient.SendAsync(roleRequest, cancellationToken);
        if (!roleResponse.IsSuccessStatusCode)
        {
            _logger.LogError("Realm role {Role} not found in Keycloak ({Status}).", roleName, roleResponse.StatusCode);
            return CustomResult.Failure($"Role '{roleName}' does not exist");
        }

        using var roleDoc = JsonDocument.Parse(await roleResponse.Content.ReadAsStringAsync(cancellationToken));
        var role = new
        {
            id = roleDoc.RootElement.GetProperty("id").GetString(),
            name = roleDoc.RootElement.GetProperty("name").GetString()
        };

        using var assignRequest = new HttpRequestMessage(HttpMethod.Post, $"{_options.AdminUsersPath}/{userId}/role-mappings/realm")
        {
            Content = JsonContent.Create(new[] { role })
        };
        assignRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        using var assignResponse = await _httpClient.SendAsync(assignRequest, cancellationToken);
        if (!assignResponse.IsSuccessStatusCode)
        {
            var body = await assignResponse.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Assigning role {Role} to {UserId} failed ({Status}): {Body}", roleName, userId, assignResponse.StatusCode, body);
            return CustomResult.Failure("Could not assign the role");
        }

        return CustomResult.Success();
    }


    public async Task<CustomResult<KeycloakUser>> FindUserAsync(string emailOrUsername, CancellationToken cancellationToken = default)
    {
        var adminToken = await GetAdminTokenAsync(cancellationToken);
        if (adminToken is null)
            return CustomResult<KeycloakUser>.Failure(CustomError.Failure("Could not authenticate to Keycloak"));

        // Email first, since that is what an invited user registers with. Both searches pass
        // exact=true, or "admin@x" would also match "admin@xyz".
        var byEmail = await QueryUserAsync("email", emailOrUsername, adminToken, cancellationToken);
        if (byEmail.IsFailure)
            return CustomResult<KeycloakUser>.Failure(byEmail.CustomError);

        if (byEmail.Value is not null)
            return CustomResult<KeycloakUser>.Success(byEmail.Value);

        var byUsername = await QueryUserAsync("username", emailOrUsername, adminToken, cancellationToken);
        if (byUsername.IsFailure)
            return CustomResult<KeycloakUser>.Failure(byUsername.CustomError);

        return byUsername.Value is null
            ? CustomResult<KeycloakUser>.Failure(CustomError.Failure("User not found"))
            : CustomResult<KeycloakUser>.Success(byUsername.Value);
    }


    public async Task<CustomResult> ResetPasswordAsync(string userId, string newPassword, CancellationToken cancellationToken = default)
    {
        var adminToken = await GetAdminTokenAsync(cancellationToken);
        if (adminToken is null)
            return CustomResult.Failure("Could not authenticate to Keycloak");

        using var request = new HttpRequestMessage(HttpMethod.Put, $"{_options.AdminUsersPath}/{userId}/reset-password")
        {
            Content = JsonContent.Create(new { type = "password", value = newPassword, temporary = false })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            // A realm password policy rejects the new password here with a 400 and a reason.
            _logger.LogError("reset-password failed for {UserId} ({Status}): {Body}", userId, response.StatusCode, body);
            return CustomResult.Failure("Could not set the new password");
        }

        return CustomResult.Success();
    }


    /// <summary>
    /// One exact-match Admin REST user search. A successful search with no match is a null value
    /// rather than a failure, so the caller can try the next field.
    /// </summary>
    private async Task<CustomResult<KeycloakUser?>> QueryUserAsync(string field, string value, string adminToken, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{_options.AdminUsersPath}?{field}={Uri.EscapeDataString(value)}&exact=true");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Keycloak user lookup by {Field} failed ({Status}).", field, response.StatusCode);
            return CustomResult<KeycloakUser?>.Failure(CustomError.Failure("Could not look the user up"));
        }

        var users = await response.Content.ReadFromJsonAsync<List<KeycloakUser>>(cancellationToken);

        return CustomResult<KeycloakUser?>.Success(users?.FirstOrDefault());
    }


    private async Task<CustomResult<KeycloakTokenResponse>> RequestTokenAsync(Dictionary<string, string> form, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsync(_options.TokenPath, new FormUrlEncodedContent(form), cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogWarning("Keycloak token request failed ({Status}): {Body}", response.StatusCode, body);

            // Keycloak explains itself in error_description, and that explanation is often nothing
            // to do with the password: "Account is not fully set up" for a user missing a required
            // profile field, "Account disabled", "Invalid client credentials" for a bad secret.
            // Flattening them all to a wrong-password message hides the actual fault.
            var description = ReadErrorDescription(body);

            return CustomResult<KeycloakTokenResponse>.Failure(CustomError.Failure(
                description ?? "Username or password not correct"));
        }

        var token = await response.Content.ReadFromJsonAsync<KeycloakTokenResponse>(cancellationToken);

        return token is null
            ? CustomResult<KeycloakTokenResponse>.Failure(CustomError.Failure("Keycloak returned an empty token response"))
            : CustomResult<KeycloakTokenResponse>.Success(token);
    }


    /// <summary>
    /// Pulls <c>error_description</c> out of a Keycloak error body. Returns null when there is
    /// nothing useful to show — including the plain wrong-password case, which the caller reports
    /// in its own words rather than as Keycloak's "Invalid user credentials".
    /// </summary>
    private static string? ReadErrorDescription(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return null;

        try
        {
            using var document = JsonDocument.Parse(body);

            if (!document.RootElement.TryGetProperty("error_description", out var element))
                return null;

            var description = element.GetString();

            if (string.IsNullOrWhiteSpace(description) ||
                description.Equals("Invalid user credentials", StringComparison.OrdinalIgnoreCase))
                return null;

            return description;
        }
        catch (JsonException)
        {
            // Not every failure comes back as JSON (a proxy or a 500 page, say).
            return null;
        }
    }


    /// <summary>
    /// Client-credentials token for the taskmaster-auth service account, cached until 30s before it
    /// expires. Returns null if it cannot be obtained.
    /// </summary>
    private async Task<string?> GetAdminTokenAsync(CancellationToken cancellationToken)
    {
        if (_adminToken is not null && DateTimeOffset.UtcNow < _adminTokenExpiresAt)
            return _adminToken;

        await _adminTokenLock.WaitAsync(cancellationToken);
        try
        {
            if (_adminToken is not null && DateTimeOffset.UtcNow < _adminTokenExpiresAt)
                return _adminToken;

            var result = await RequestTokenAsync(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = _options.ClientId,
                ["client_secret"] = _options.ClientSecret
            }, cancellationToken);

            if (result.IsFailure)
                return null;

            _adminToken = result.Value.AccessToken;
            _adminTokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(Math.Max(result.Value.ExpiresIn - 30, 5));

            return _adminToken;
        }
        finally
        {
            _adminTokenLock.Release();
        }
    }
}
