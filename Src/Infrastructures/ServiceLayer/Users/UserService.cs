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


    public async Task<CustomResult> AddUser(CreateUserRequest createUserRequest)
    {
        var newUser = new User()
        {
            FullName = createUserRequest.FullName,
            UserEmail = createUserRequest.UserEmail,
            UserTypeId = (int)UserType.AdminUser,
            RegisterTokenValid = DateTime.UtcNow.AddHours(2)
        };

        var myReturn = await _userRepository.AddAsync(newUser);

        if (myReturn == null) CustomResult.Failure("Not saved");

        return CustomResult.Success();
    }

    public Task<CustomResult<LoginResponse>> GetUserByAccessTokenAsync(string accessToken)
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

    public Task<IEnumerable<UserDto>> GetUsers(bool IsActive, int UserTypeId)
    {
        throw new NotImplementedException();
    }
}
