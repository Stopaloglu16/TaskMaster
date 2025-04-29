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
    Task<CustomResult<UserDto>> GetUserByUserGuidId(Guid UserGuidId);

    Task<bool> UpdateRefreshTokenAsync(int UserId, string refreshToken, DateTime refreshTokenExpiery);

    Task<CustomError> CheckRefreshTokenOfUser(Guid userGuidId, string refreshToken);

    Task<PagingResponse<UserDto>> GetActiveUsersWithPagination(PagingParameters pagingParameters, CancellationToken cancellationToken);

}
