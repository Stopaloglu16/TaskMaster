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


    public async Task<CustomResult<string?>> SoftDeleteUserAsync(int UserId)
    {
        // Soft delete rather than the generic EfCoreRepository.DeleteAsync: that one is a hard
        // Remove, and FK_TaskLists_Users_AssignedToId has no ON DELETE, so removing a user who owns
        // any list throws. Loading the graph and saving once also keeps the whole cascade in a
        // single transaction, which a per-call repository could not.
        var user = await _dbContext.Users
                                   .Include(u => u.TaskLists!)
                                       .ThenInclude(tl => tl.TaskItems!)
                                   .FirstOrDefaultAsync(u => u.Id == UserId);

        if (user is null)
            return CustomResult<string?>.Failure(new CustomError(false, "Not found to delete"));

        var aspId = user.AspId;

        user.IsDeleted = 1;

        // 2 is BaseEntity's "parent deleted" marker: the rows go away with their owner but stay
        // distinguishable from something deleted in its own right.
        foreach (var taskList in user.TaskLists ?? Enumerable.Empty<TaskList>())
        {
            taskList.IsDeleted = 2;

            foreach (var taskItem in taskList.TaskItems)
            {
                taskItem.IsDeleted = 2;
            }
        }

        await _dbContext.SaveChangesAsync(CancellationToken.None);

        return CustomResult<string?>.Success(aspId);
    }

    public async Task<CustomResult<UserDto>> GetUserByAspId(string AspId)
    {
        var myUserDto = await _dbContext.Users.Where(uu => uu.AspId == AspId)
                                                  .AsNoTracking()
                                                  .Select(ss => new UserDto()
                                                  {
                                                      Id = ss.Id,
                                                      AspId = ss.AspId,
                                                      FullName = ss.FullName,
                                                      UserEmail = ss.UserEmail,
                                                      UserType = (UserType)ss.UserTypeId
                                                  })
                                                  .FirstOrDefaultAsync();

        if (myUserDto == null) return CustomResult<UserDto>.Failure(CustomError.Failure("The user not found"));

        return CustomResult<UserDto>.Success(myUserDto);
    }

    //public async Task<CustomResult<UserDto>> GetUserByUserGuidId(Guid UserGuidId)
    //{
    //    var myUserDto = await _dbContext.Users.Where(uu => uu.UserGuidId == UserGuidId)
    //                                              .AsNoTracking()
    //                                              .Select(ss => new UserDto()
    //                                              {
    //                                                  Id = ss.Id,
    //                                                  UserGuidId = ss.UserGuidId,
    //                                                  FullName = ss.FullName,
    //                                                  UserEmail = ss.UserEmail,
    //                                                  UserType = (UserType)ss.UserTypeId
    //                                              })
    //                                              .FirstOrDefaultAsync();

    //    if (myUserDto == null) return CustomResult<UserDto>.Failure(CustomError.Failure("The user not found"));

    //    return CustomResult<UserDto>.Success(myUserDto);
    //}

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
        // compute target deleted flag once to avoid repeated conversions in the query
        byte expectedIsDeleted = IsActive ? (byte)0 : (byte)1;

        var userList = await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.UserTypeId == UserTypeId && u.IsDeleted == expectedIsDeleted)
            .Select(u => new UserDto
            {
                Id = u.Id,
                FullName = u.FullName
            })
            .ToListAsync()
            .ConfigureAwait(false);

        return userList;
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

    public async Task<CustomError> CheckRefreshTokenOfUser(string aspId, string refreshToken)
    {
        var tokenTemp = await _dbContext.Users.Where(uu => uu.AspId == aspId)
                                     .Select(ss => new { ss.RefreshToken, ss.RefreshTokenExpiryTime })
                                     .FirstAsync();

        if (tokenTemp == null)
            return CustomError.Failure("user not found");

        if (tokenTemp.RefreshToken != refreshToken || tokenTemp.RefreshTokenExpiryTime <= DateTime.Now)
            return CustomError.Failure("Invalid token");

        return CustomError.Success();
    }
}
