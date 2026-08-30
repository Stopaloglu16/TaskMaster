using Application.Aggregates.UserAuthAggregate;
using Application.Common.Models;
using System.IdentityModel.Tokens.Jwt;

namespace WebApiAuth.Services;

/// <summary>
/// Brokers login and refresh to Keycloak. WebApiAuth no longer signs tokens itself — the realm does,
/// with RS256, and WebApi validates them against the realm's JWKS.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IKeycloakClient _keycloakClient;

    public AuthService(IKeycloakClient keycloakClient)
    {
        _keycloakClient = keycloakClient;
    }


    public async Task<CustomResult<UserLoginResponse>> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var token = await _keycloakClient.PasswordGrantAsync(username, password, cancellationToken);

        return token.IsFailure
            ? CustomResult<UserLoginResponse>.Failure(token.CustomError)
            : CustomResult<UserLoginResponse>.Success(ToLoginResponse(token.Value));
    }


    public async Task<CustomResult<UserLoginResponse>> RefreshTokensAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var token = await _keycloakClient.RefreshAsync(refreshToken, cancellationToken);

        return token.IsFailure
            ? CustomResult<UserLoginResponse>.Failure(token.CustomError)
            : CustomResult<UserLoginResponse>.Success(ToLoginResponse(token.Value));
    }


    public string? GetSubject(string accessToken)
    {
        return ReadClaim(accessToken, JwtRegisteredClaimNames.Sub);
    }


    private static UserLoginResponse ToLoginResponse(KeycloakTokenResponse token)
    {
        return new UserLoginResponse
        {
            AccessToken = token.AccessToken,
            RefreshToken = token.RefreshToken,
            // preferred_username, not "name": Keycloak puts the full name in the latter.
            UserName = ReadClaim(token.AccessToken, "preferred_username") ?? string.Empty
        };
    }


    /// <summary>
    /// Reads a claim without validating the signature. Safe here: the token came straight from
    /// Keycloak over the client's own authenticated channel, and WebApi validates it properly
    /// before honouring it.
    /// </summary>
    private static string? ReadClaim(string accessToken, string claimType)
    {
        var handler = new JwtSecurityTokenHandler();

        if (!handler.CanReadToken(accessToken))
            return null;

        return handler.ReadJwtToken(accessToken).Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
    }
}
