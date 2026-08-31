using Application.Common.Models;
using Domain.Entities;

namespace Application.Repositories;

public interface IUserRegisterRepository
{
    Task<CustomResult<User>> GetUserByAsync(string Username, string Token);

    Task<CustomResult<User>> GetUserByAsync(string Username);

    Task<CustomResult> UpdateUserAsync(int UserId, string AspId);

    /// <summary>Invalidates the outstanding invite/reset token so a used link cannot be replayed.</summary>
    Task<CustomResult> ExpireRegisterTokenAsync(int UserId);
}
