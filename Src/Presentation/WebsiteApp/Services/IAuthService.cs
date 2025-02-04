using Application.Aggregates.UserAuthAggregate;
using Application.Aggregates.UserAuthAggregate.Token;
using Application.Common.Models;

namespace WebsiteApp.Services;

public interface IAuthService
{
    public Task<CustomResult<UserLoginResponse>> LoginAsync(UserLoginRequest loginRequest);
    public Task<CustomResult> RegisterUserAsync(RegisterUserRequest registerUserRequest);
    public Task<UserLoginResponse> GetUserByTokenAsync(RefreshTokenRequest tokenRefreshRequest);
}
