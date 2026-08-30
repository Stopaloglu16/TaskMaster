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


    public async Task<CustomResult<string>> CreateUserAsync(string username, string email, string password, CancellationToken cancellationToken = default)
    {
        var adminToken = await GetAdminTokenAsync(cancellationToken);
        if (adminToken is null)
            return CustomResult<string>.Failure(CustomError.Failure("Could not authenticate to Keycloak"));

        var payload = new
        {
            username,
            email,
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

        if (response.StatusCode == HttpStatusCode.Conflict)
            return CustomResult<string>.Failure(CustomError.Failure("A user with that name or email already exists"));

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Keycloak user creation failed ({Status}): {Body}", response.StatusCode, body);
            return CustomResult<string>.Failure(CustomError.Failure("Could not create the user"));
        }

        // Keycloak returns 201 with no body; the new id is the last segment of the Location header.
        var location = response.Headers.Location?.ToString();
        var userId = location?.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();

        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogError("Keycloak user creation returned no Location header.");
            return CustomResult<string>.Failure(CustomError.Failure("Could not create the user"));
        }

        return CustomResult<string>.Success(userId);
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


    public async Task<CustomResult<KeycloakUser>> FindUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var adminToken = await GetAdminTokenAsync(cancellationToken);
        if (adminToken is null)
            return CustomResult<KeycloakUser>.Failure(CustomError.Failure("Could not authenticate to Keycloak"));

        // exact=true, or "admin@x" would also match "admin@xyz".
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{_options.AdminUsersPath}?email={Uri.EscapeDataString(email)}&exact=true");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Keycloak user lookup failed ({Status}).", response.StatusCode);
            return CustomResult<KeycloakUser>.Failure(CustomError.Failure("Could not look the user up"));
        }

        var users = await response.Content.ReadFromJsonAsync<List<KeycloakUser>>(cancellationToken);
        var user = users?.FirstOrDefault();

        return user is null
            ? CustomResult<KeycloakUser>.Failure(CustomError.Failure("User not found"))
            : CustomResult<KeycloakUser>.Success(user);
    }


    public async Task<CustomResult> SendUpdatePasswordEmailAsync(string userId, CancellationToken cancellationToken = default)
    {
        var adminToken = await GetAdminTokenAsync(cancellationToken);
        if (adminToken is null)
            return CustomResult.Failure("Could not authenticate to Keycloak");

        // 24h to match the old hand-rolled reset window.
        using var request = new HttpRequestMessage(HttpMethod.Put, $"{_options.AdminUsersPath}/{userId}/execute-actions-email?lifespan=86400")
        {
            Content = JsonContent.Create(new[] { "UPDATE_PASSWORD" })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            // The usual cause is the realm having no SMTP server configured.
            _logger.LogError("execute-actions-email failed for {UserId} ({Status}): {Body}", userId, response.StatusCode, body);
            return CustomResult.Failure("Could not send the reset email");
        }

        return CustomResult.Success();
    }


    private async Task<CustomResult<KeycloakTokenResponse>> RequestTokenAsync(Dictionary<string, string> form, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsync(_options.TokenPath, new FormUrlEncodedContent(form), cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            // 401 is an ordinary bad password; anything else is worth a log line.
            if (response.StatusCode != HttpStatusCode.Unauthorized)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Keycloak token request failed ({Status}): {Body}", response.StatusCode, body);
            }

            return CustomResult<KeycloakTokenResponse>.Failure(CustomError.Failure("Username or password not correct"));
        }

        var token = await response.Content.ReadFromJsonAsync<KeycloakTokenResponse>(cancellationToken);

        return token is null
            ? CustomResult<KeycloakTokenResponse>.Failure(CustomError.Failure("Keycloak returned an empty token response"))
            : CustomResult<KeycloakTokenResponse>.Success(token);
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
