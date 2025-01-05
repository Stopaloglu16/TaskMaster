using Application.Aggregates.UserAggregate.Queries;
using Application.Aggregates.UserAuthAggregate;
using Application.Aggregates.UserAuthAggregate.Token;
using Application.Common.Models;
using Azure;

namespace WebApp.Services;

public interface IAuthService
{
    public Task<UserLoginResponse> LoginAsync(UserLoginRequest loginRequest);
    public Task<CustomResult> RegisterUserAsync(RegisterUserRequest registerUserRequest);
    public Task<UserLoginResponse> GetUserByAccessTokenAsync(TokenRefreshRequest tokenRefreshRequest);
}
