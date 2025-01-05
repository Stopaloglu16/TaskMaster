using Application.Aggregates.UserAggregate.Queries;
using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities;
using Domain.Enums;

namespace Application.Repositories;

public interface IUserRepository : IRepository<User, int>
{
    Task<IEnumerable<UserDto>> GetUsers(bool IsActive, UserType userType);
    Task<IEnumerable<SelectListItem>> GetTaskUserSelectList();

    Task<CustomResult<UserDto>> GetUserById(int Id);

    Task<CustomResult<UserDto>> GetUserByAspId(string AspId);

    Task<bool> SaveRefreshTokenAsync(RefreshToken refreshToken, int UserId);

    Task<RefreshToken> GetRefreshToken(string tokenRequest);

    Task<PagingResponse<UserDto>> GetActiveUsersWithPagination(PagingParameters pagingParameters, CancellationToken cancellationToken);

}
