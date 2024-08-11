using Application.Aggregates.UserAggregate.Queries;
using Application.Common.Models;
using Application.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserRepository : EfCoreRepository<User, int>, IUserRepository
{

    private readonly ApplicationDbContext _dbContext;

    public UserRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CustomResult<UserDto>> GetUserByAspId(string AspId)
    {
        User? myuser = await _dbContext.Users.Where(uu => uu.AspId == AspId)
                                                .FirstOrDefaultAsync();

        if (myuser?.Id == 0) return CustomResult<UserDto>.Failure(CustomError.Failure("The user not found"));

        var myUserDto = new UserDto()
        {
            FullName = myuser.FullName,
            UserEmail = myuser.UserEmail,
            UserType = (UserType)myuser.UserTypeId
        };


        return CustomResult<UserDto>.Success(myUserDto);
    }

    public async Task<CustomResult<UserDto>> GetUserById(int Id)
    {
        var myuser = await _dbContext.Users.Where(uu => uu.Id == Id)
                                             .FirstOrDefaultAsync();

        if (myuser?.Id == 0) return CustomResult<UserDto>.Failure(CustomError.Failure("The user not found"));

        var myUserDto = new UserDto()
        {
            FullName = myuser.FullName,
            UserEmail = myuser.UserEmail,
            UserType = (UserType)myuser.UserTypeId
        };


        return CustomResult<UserDto>.Success(myUserDto);
    }

    public async Task<IEnumerable<UserDto>> GetUsers(bool IsActive, int UserTypeId)
    {
        return await _dbContext.Users.Where(uu => uu.UserTypeId == (UserType)UserTypeId)
                                           .Select(ss => new UserDto()
                                           {
                                               Id = ss.Id,
                                               FullName = ss.FullName
                                           }).ToListAsync();
    }
}
