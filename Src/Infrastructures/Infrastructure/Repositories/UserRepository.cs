using Application.Aggregates.TaskListAggregate.Queries;
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
                                                     Id= ss.Id,
                                                     FullName = ss.FullName,
                                                     UserEmail = ss.UserEmail,
                                                     UserType = (UserType)ss.UserTypeId
                                                 })
                                                 .FirstOrDefaultAsync();

        if (myUserDto == null) return CustomResult<UserDto>.Failure(CustomError.Failure("The user not found"));

        return CustomResult<UserDto>.Success(myUserDto);
    }

    public async Task<IEnumerable<UserDto>> GetUsers(bool IsActive, UserType UserTypeId)
    {
        return await _dbContext.Users.Where(uu => uu.UserTypeId == UserTypeId &&
                                                      uu.IsDeleted == Convert.ToByte(!IsActive))
                                     .AsNoTracking()
                                     .Select(ss => new UserDto()
                                     {
                                         Id = ss.Id,
                                         FullName = ss.FullName
                                     }).ToListAsync();
    }

    public async Task<IEnumerable<SelectListItem>> GetTaskUserSelectList()
    {
        return await _dbContext.Users.Where(uu => uu.UserTypeId == UserType.TaskUser &&
                                                      uu.IsDeleted == 0)
                                     .AsNoTracking()
                                     .Select(ss => new SelectListItem()
                                     {
                                         Value = ss.Id,
                                         Text = ss.FullName
                                     }).ToListAsync();
    }

    public async Task<bool> SaveRefreshTokenAsync(RefreshToken refreshToken, int _UserId)
    {
        _dbContext.RefreshTokens.Add(
                   new RefreshToken
                   {
                       UserId = _UserId,
                       Token = refreshToken.Token,
                       ExpiryDate = refreshToken.ExpiryDate,
                       IsRevoked = false,
                       IsUsed = false
                   });

        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<RefreshToken> GetRefreshToken(string tokenRequest)
    {
        return await _dbContext.RefreshTokens.FirstOrDefaultAsync(t => t.Token == tokenRequest);
    }


    public async Task<PagingResponse<UserDto>> GetActiveUsersWithPagination(PagingParameters pagingParameters,
                                                                                CancellationToken cancellationToken)
    {
        var query = _dbContext.Users.AsNoTracking()
                                      .Where(qq => qq.IsDeleted == 0)
                                         .Select(ss => new UserDto
                                         {
                                             Id = ss.Id,
                                             FullName = ss.FullName,
                                             UserEmail = ss.UserEmail,
                                             UserType = ss.UserTypeId
                                         });

        return await PagingResponse<UserDto>.CreateAsync(query, pagingParameters);
    }
}
