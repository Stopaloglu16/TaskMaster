using Application.Aggregates.UserAggregate.Queries;
using Application.Common.Interfaces;
using Domain.Entities;

namespace Application.Repositories;

public interface IUserRepository : IRepository<User, int>
{
    Task<IEnumerable<UserDto>> GetUsers(bool IsActive, int UserTypeId);

    Task<UserDto> GetUserById(int Id);

    Task<UserDto> GetUserByAspId(string AspId);

}
