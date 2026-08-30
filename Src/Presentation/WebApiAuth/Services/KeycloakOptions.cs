namespace WebApiAuth.Services;

/// <summary>
/// Bound from the "Keycloak" configuration section, which the AppHost injects as environment
/// variables. Nothing here has a default: a missing value should fail loudly at startup rather than
/// produce 401s no one can explain.
/// </summary>
public sealed class KeycloakOptions
{
    public const string SectionName = "Keycloak";

    /// <summary>Root of the Keycloak server, e.g. <c>http://localhost:8080</c>.</summary>
    public string BaseUrl { get; set; } = string.Empty;

    public string Realm { get; set; } = string.Empty;

    /// <summary>The confidential client WebApiAuth brokers through (taskmaster-auth).</summary>
    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>Realm issuer URL — must match the <c>iss</c> WebApi validates against.</summary>
    public string Authority { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public string TokenPath => $"realms/{Realm}/protocol/openid-connect/token";

    public string AdminUsersPath => $"admin/realms/{Realm}/users";

    public string AdminRealmRolesPath => $"admin/realms/{Realm}/roles";
}
