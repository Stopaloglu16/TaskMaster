using Application.Aggregates.UserAggregate.Commands;
using Application.Aggregates.UserAggregate.Queries;
using Application.Aggregates.UserAuthAggregate;
using Application.Common.Models;

namespace ServiceLayer.Users;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetUsers(bool IsActive, int UserTypeId);

    Task<CustomResult<UserDto>> GetUserById(int Id);

    Task<CustomResult<UserDto>> GetUserByAspId(string AspId);

    public Task<CustomResult<LoginResponse>> GetUserByAccessTokenAsync(string accessToken);

    Task<CustomResult> AddUser(CreateUserRequest createUserRequest);

}