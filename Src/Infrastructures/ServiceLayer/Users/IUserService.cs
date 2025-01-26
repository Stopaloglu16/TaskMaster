using Application.Aggregates.UserAggregate.Commands;
using Application.Aggregates.UserAggregate.Queries;
using Application.Aggregates.UserAuthAggregate;
using Application.Common.Models;
using Domain.Entities;
using Domain.Enums;

namespace ServiceLayer.Users;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetUsers(bool IsActive, UserType UserType);
    Task<IEnumerable<SelectListItem>> GetTaskUserSelectList();
    Task<CustomResult<UserDto>> GetUserById(int Id);

    Task<CustomResult<UserDto>> GetUserByAspId(string AspId);
    Task<CustomResult<UserDto>> GetUserByUserGuidId(Guid UserGuidId);

    public Task<CustomResult<UserLoginResponse>> GetUserByAccessTokenAsync(string accessToken);


    Task<CustomResult<Guid>> AddUser(CreateUserRequest createUserRequest);
    Task<CustomResult> UpdateUser(int Id, UpdateUserRequest updateUserRequest);

    Task<CustomResult<Guid>> RefreshRegisterToken(int Id);

    Task<bool> UpdateRefreshTokenAsync(int UserId, string refreshToken, DateTime refreshTokenExpiery);

    Task<CustomError> CheckRefreshTokenOfUser(Guid userGuidId, string refreshToken);

    Task<PagingResponse<UserDto>> GetActiveUsersWithPagination(PagingParameters pagingParameters, CancellationToken cancellationToken);

}