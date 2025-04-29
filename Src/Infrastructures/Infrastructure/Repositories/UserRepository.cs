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
                                                      UserGuidId = ss.UserGuidId,
                                                      FullName = ss.FullName,
                                                      UserEmail = ss.UserEmail,
                                                      UserType = (UserType)ss.UserTypeId
                                                  })
                                                  .FirstOrDefaultAsync();

        if (myUserDto == null) return CustomResult<UserDto>.Failure(CustomError.Failure("The user not found"));

        return CustomResult<UserDto>.Success(myUserDto);
    }

    public async Task<CustomResult<UserDto>> GetUserByUserGuidId(Guid UserGuidId)
    {
        var myUserDto = await _dbContext.Users.Where(uu => uu.UserGuidId == UserGuidId)
                                                  .AsNoTracking()
                                                  .Select(ss => new UserDto()
                                                  {
                                                      Id = ss.Id,
                                                      UserGuidId = ss.UserGuidId,
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
                                                     Id = ss.Id,
                                                     FullName = ss.FullName,
                                                     UserEmail = ss.UserEmail,
                                                     UserType = (UserType)ss.UserTypeId
                                                 })
                                                 .FirstOrDefaultAsync();

        if (myUserDto == null) return CustomResult<UserDto>.Failure(CustomError.Failure("The user not found"));

        return CustomResult<UserDto>.Success(myUserDto);
    }

    public async Task<CustomResult<User>> GetUserByEmail(string email)
    {
        var currentUser = await _dbContext.Users.Where(uu => uu.UserEmail == email)
                                               .AsNoTracking()
                                               .FirstOrDefaultAsync();

        if (currentUser == null) return CustomResult<User>.Failure(CustomError.Failure("The user not found"));

        return CustomResult<User>.Success(currentUser);
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
        var uu = await _dbContext.Users.Where(uu => uu.UserTypeId == UserType.TaskUser &&
                                                      uu.IsDeleted == 0)
                                     .AsNoTracking()
                                     .Select(ss => new SelectListItem()
                                     {
                                         Value = ss.Id,
                                         Text = ss.FullName
                                     }).ToListAsync();

        return uu;
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

    public async Task<bool> UpdateRefreshTokenAsync(int UserId, string refreshToken, DateTime refreshTokenExpiery)
    {
        var currenctUser = await _dbContext.Users.FirstOrDefaultAsync(uu => uu.Id == UserId);

        if (currenctUser == null)
            throw new ArgumentNullException();

        currenctUser.RefreshToken = refreshToken;
        currenctUser.RefreshTokenExpiryTime = refreshTokenExpiery;

        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<CustomError> CheckRefreshTokenOfUser(Guid userGuidId, string refreshToken)
    {
        var tokenTemp = await _dbContext.Users.Where(uu => uu.UserGuidId == userGuidId)
                                     .Select(ss => new { ss.RefreshToken, ss.RefreshTokenExpiryTime })
                                     .FirstAsync();

        if (tokenTemp == null)
            return CustomError.Failure("user not found");

        if (tokenTemp.RefreshToken != refreshToken || tokenTemp.RefreshTokenExpiryTime <= DateTime.Now)
            return CustomError.Failure("Invalid token");

        return CustomError.Success();
    }
}
