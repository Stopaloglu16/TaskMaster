using Application.Aggregates.UserAuthAggregate;
using Application.Common.Models;

namespace WebApiAuth.Services;

public interface IAuthService
{
    Task<CustomResult<UserLoginResponse>> LoginAsync(string username, string password, CancellationToken cancellationToken = default);

    Task<CustomResult<UserLoginResponse>> RefreshTokensAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads the <c>sub</c> claim — the Keycloak user id — out of an access token. This is the value
    /// stored in <c>Domain.Entities.User.AspId</c> and stamped into audit columns.
    /// </summary>
    string? GetSubject(string accessToken);
}
