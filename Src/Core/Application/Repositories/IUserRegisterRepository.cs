using Application.Common.Models;
using Domain.Entities;

namespace Application.Repositories;

public interface IUserRegisterRepository
{
    Task<CustomResult<User>> GetUserByAsync(string Username, string Token);

    Task<CustomResult> UpdateUserAsync(int UserId, string AspId);
}
