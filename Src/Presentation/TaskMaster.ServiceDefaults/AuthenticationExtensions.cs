using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace TaskMaster.ServiceDefaults;


public static class AuthenticationExtensions
{
    /// <summary>
    /// Claim name of the flattened realm-role list. Keycloak's default is the nested
    /// <c>realm_access.roles</c>, which <see cref="TokenValidationParameters.RoleClaimType"/> cannot
    /// address — the taskmaster-auth client carries a "User Realm Role" mapper that projects it to a
    /// flat <c>roles</c> claim instead. See Keycloak/taskmaster-realm.json in the AppHost.
    /// </summary>
    public const string RoleClaimType = "roles";

    /// <summary>Keycloak's username claim; the OIDC <c>name</c> claim holds the full name.</summary>
    public const string NameClaimType = "preferred_username";

    public static IServiceCollection AddDefaultAuthentication(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        //"Keycloak": {
        //  "Authority": "http://localhost:8080/realms/taskmaster",
        //  "Audience": "taskmaster-api"
        //}
        // Both are injected by the AppHost; GetRequiredValue throws if they are missing.
        var keycloak = builder.Configuration.GetSection("Keycloak");

        services.AddAuthentication(o =>
        {
            o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

        }).AddJwtBearer(x =>
        {
            // Authority drives OIDC discovery, so the RS256 signing keys come from the realm's JWKS
            // and rotate with it. No shared secret anywhere.
            x.Authority = keycloak.GetRequiredValue("Authority");
            x.Audience = keycloak.GetRequiredValue("Audience");

            // Keycloak is served over plain HTTP on the pinned :8080 in development.
            x.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
            x.SaveToken = true;

            // Leaves sub -> ClaimTypes.NameIdentifier, which CurrentUserService reads.
            x.MapInboundClaims = true;

            x.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = keycloak.GetRequiredValue("Authority"),
                ValidAudience = keycloak.GetRequiredValue("Audience"),
                NameClaimType = NameClaimType,
                RoleClaimType = RoleClaimType,
                // Not zero: clock drift between the Keycloak container and the host would otherwise
                // reject freshly issued tokens.
                ClockSkew = TimeSpan.FromSeconds(30)
            };
        });

        services.AddAuthorization();

        return services;
    }
}
