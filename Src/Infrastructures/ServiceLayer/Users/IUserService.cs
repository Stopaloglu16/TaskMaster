using Application.Aggregates.UserAggregate.Commands;
using Application.Aggregates.UserAggregate.Queries;
using Application.Aggregates.UserAuthAggregate;
using Application.Common.Models;
using Domain.Entities;

namespace ServiceLayer.Users;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetUsers(bool IsActive, int UserTypeId);

    Task<CustomResult<UserDto>> GetUserById(int Id);

    Task<CustomResult<UserDto>> GetUserByAspId(string AspId);

    public Task<CustomResult<UserLoginResponse>> GetUserByAccessTokenAsync(string accessToken);

    Task<CustomResult<Guid>> AddUser(CreateUserRequest createUserRequest);

    Task<bool> SaveRefreshTokenAsync(RefreshToken refreshToken, int UserId);

    Task<RefreshToken> GetRefreshToken(string tokenRequest);

}