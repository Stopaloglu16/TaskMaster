using Application.Common.Models;
using Domain.Entities;

namespace ServiceLayer.Users;

public interface IUserRegisterService
{
    Task<CustomResult<User>> GetUserByAsync(string Username, string Token);

    Task<CustomResult> UpdateUserAsync(int UserId, string AspId);

}
