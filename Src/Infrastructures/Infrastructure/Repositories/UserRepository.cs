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
        var myUserDto = await _dbContext.Users.Where(uu => uu.AspId == AspId)
                                                  .AsNoTracking()
                                                  .Select(ss => new UserDto()
                                                  {
                                                      Id = ss.Id,
                                                      FullName = ss.FullName,
                                                      UserEmail = ss.UserEmail,
                                                      UserType = (UserType)ss.UserTypeId
                                                  })
                                                  .FirstOrDefaultAsync();

        if (myUserDto == null) return CustomResult<UserDto>.Failure(CustomError.Failure("The user not found"));

        return CustomResult<UserDto>.Success(myUserDto);
    }

    public async Task<CustomResult<UserDto>> GetUserById(int Id)
    {
        var myUserDto = await _dbContext.Users.Where(uu => uu.Id == Id)
                                                 .AsNoTracking()
                                                 .Select(ss => new UserDto()
                                                 {
                                                     FullName = ss.FullName,
                                                     UserEmail = ss.UserEmail,
                                                     UserType = (UserType)ss.UserTypeId
                                                 })
                                                 .FirstOrDefaultAsync();

        if (myUserDto == null) return CustomResult<UserDto>.Failure(CustomError.Failure("The user not found"));

        return CustomResult<UserDto>.Success(myUserDto);
    }

    public async Task<IEnumerable<UserDto>> GetUsers(bool IsActive, int UserTypeId)
    {
        return await _dbContext.Users.Where(uu => uu.UserTypeId == (UserType)UserTypeId)
                                     .AsNoTracking()
                                     .Select(ss => new UserDto()
                                     {
                                         Id = ss.Id,
                                         FullName = ss.FullName
                                     }).ToListAsync();
    }

    public async Task<bool> SaveRefreshTokenAsync(RefreshToken refreshToken, int _UserId)
    {
        _dbContext.RefreshTokens.Add(
                   new RefreshToken
                   {
                       UserId = _UserId,
                       Token = refreshToken.Token,
                       ExpiryDate = refreshToken.ExpiryDate
                   });

        await _dbContext.SaveChangesAsync();

        return true;
    }
}
