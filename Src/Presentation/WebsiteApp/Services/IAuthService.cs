using Application.Aggregates.UserAuthAggregate;
using Application.Aggregates.UserAuthAggregate.Token;
using Application.Common.Models;
using ForgotPasswordRequest = Application.Aggregates.UserAuthAggregate.ForgotPasswordRequest;

namespace WebsiteApp.Services;

public interface IAuthService
{
    public Task<CustomResult<UserLoginResponse>> LoginAsync(UserLoginRequest loginRequest);
    public Task<CustomResult> RegisterUserAsync(RegisterUserRequest registerUserRequest);
    public Task<UserLoginResponse> GetUserByTokenAsync(RefreshTokenRequest tokenRefreshRequest);
    public Task<CustomResult> ForgotPasswordRequestAsync(ForgotPasswordRequest forgotPasswordRequest);
}
