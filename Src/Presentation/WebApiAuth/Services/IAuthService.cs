using Application.Aggregates.UserAggregate.Queries;
using Application.Aggregates.UserAuthAggregate;
using Application.Aggregates.UserAuthAggregate.Token;
using System.Security.Claims;

namespace WebApiAuth.Services;

public interface IAuthService
{
    Task<UserLoginResponse?> LoginAsync(UserTokenDto userTokenDto);
    Task<UserLoginResponse?> RefreshTokensAsync(UserTokenDto userTokenDto);
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}
