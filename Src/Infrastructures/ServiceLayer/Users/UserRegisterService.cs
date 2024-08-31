using Application.Common.Models;
using Application.Repositories;
using Domain.Entities;

namespace ServiceLayer.Users;

public class UserRegisterService : IUserRegisterService
{

    private readonly IUserRegisterRepository _userRegisterRepository;

    public UserRegisterService(IUserRegisterRepository userRegisterRepository)
    {
        _userRegisterRepository = userRegisterRepository;
    }


    public async Task<CustomResult<User>> GetUserByAsync(string Username, string Token)
    {
        return await _userRegisterRepository.GetUserByAsync(Username, Token);
    }

    public async Task<CustomResult> UpdateUserAsync(int UserId, string AspId)
    {
        return await _userRegisterRepository.UpdateUserAsync(UserId, AspId);
    }
}
