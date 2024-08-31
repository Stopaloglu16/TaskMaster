using Application.Aggregates.UserAggregate.Queries;
using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities;

namespace Application.Repositories;

public interface IUserRepository : IRepository<User, int>
{
    Task<IEnumerable<UserDto>> GetUsers(bool IsActive, int UserTypeId);

    Task<CustomResult<UserDto>> GetUserById(int Id);

    Task<CustomResult<UserDto>> GetUserByAspId(string AspId);

    Task<bool> SaveRefreshTokenAsync(RefreshToken refreshToken, int UserId);
}
