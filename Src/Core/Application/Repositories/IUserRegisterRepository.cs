using Application.Common.Models;
using Domain.Entities;

namespace Application.Repositories;

public interface IUserRegisterRepository
{
    Task<CustomResult<User>> GetUserByAsync(string Username, string Token);

    Task<CustomResult<User>> GetUserByAsync(string Username);

    Task<CustomResult> UpdateUserAsync(int UserId, string AspId);
}
