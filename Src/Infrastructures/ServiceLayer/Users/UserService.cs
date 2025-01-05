﻿using Application.Aggregates.UserAggregate.Commands;
using Application.Aggregates.UserAggregate.Queries;
using Application.Aggregates.UserAuthAggregate;
using Application.Common.Models;
using Application.Repositories;
using Domain.Entities;
using Domain.Enums;

namespace ServiceLayer.Users;

public class UserService : IUserService
{

    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }


    public async Task<CustomResult<Guid>> AddUser(CreateUserRequest createUserRequest)
    {
        var newUser = new User()
        {
            FullName = createUserRequest.FullName,
            UserEmail = createUserRequest.UserEmail,
            UserTypeId = (int)UserType.AdminUser,
            RegisterTokenValid = DateTime.UtcNow.AddHours(2)
        };

        var newUserRepo = await _userRepository.AddAsync(newUser);

        if (newUserRepo == null) return CustomResult<Guid>.Failure(new CustomError(false, "Not saved"));

        var newUser1 = await _userRepository.GetByIdAsync(newUserRepo.Id);

        return CustomResult<Guid>.Success(newUser1.RegisterToken);
    }

 

    public Task<CustomResult<UserLoginResponse>> GetUserByAccessTokenAsync(string accessToken)
    {
        throw new NotImplementedException();
    }

    public async Task<CustomResult<UserDto>> GetUserByAspId(string AspId)
    {
        return await _userRepository.GetUserByAspId(AspId);
    }

    public async Task<CustomResult<UserDto>> GetUserById(int Id)
    {
        return await _userRepository.GetUserById(Id);
    }

    public async Task<IEnumerable<UserDto>> GetUsers(bool IsActive, UserType UserType)
    {
        return await _userRepository.GetUsers(IsActive, UserType);
    }

    public async Task<IEnumerable<SelectListItem>> GetTaskUserSelectList()
    {
        return await _userRepository.GetTaskUserSelectList();
    }

    public async Task<bool> SaveRefreshTokenAsync(RefreshToken refreshToken, int UserId)
    {
        return await _userRepository.SaveRefreshTokenAsync(refreshToken, UserId);
    }

    public async Task<RefreshToken> GetRefreshToken(string tokenRequest)
    {
        return await _userRepository.GetRefreshToken(tokenRequest);
    }
}
