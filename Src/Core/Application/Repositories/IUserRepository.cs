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
    Task<CustomResult<User>> GetUserByEmail(string email);

    Task<CustomResult<UserDto>> GetUserByAspId(string AspId);
    //Task<CustomResult<UserDto>> GetUserByUserGuidId(Guid UserGuidId);

    Task<bool> UpdateRefreshTokenAsync(int UserId, string refreshToken, DateTime refreshTokenExpiery);

    Task<CustomError> CheckRefreshTokenOfUser(string aspId, string refreshToken);

    Task<PagingResponse<UserDto>> GetActiveUsersWithPagination(PagingParameters pagingParameters, CancellationToken cancellationToken);

    /// <summary>
    /// Marks the user deleted and cascades to everything they own, in one save. Returns the
    /// Keycloak id (<c>AspId</c>) of the removed user so the caller can drop the realm account too,
    /// or a failure if there is no such user.
    /// </summary>
    Task<CustomResult<string?>> SoftDeleteUserAsync(int UserId);

}
