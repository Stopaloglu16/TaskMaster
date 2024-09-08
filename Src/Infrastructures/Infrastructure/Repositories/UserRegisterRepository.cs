using Application.Common.Models;
using Application.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserRegisterRepository : EfCoreRepository<User, int>, IUserRegisterRepository
{
    private readonly ApplicationDbContext _dbContext;
    public UserRegisterRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CustomResult<User>> GetUserByAsync(string Username, string Token)
    {
        Guid guid = Guid.Parse(Token);

        var user = await _dbContext.Users.AsNoTracking()
                                     .Where(u => u.UserEmail == Username &&
                                                     u.RegisterToken == guid)
                                     .FirstOrDefaultAsync();
        if (user is null)
            return CustomResult<User>.Failure(new CustomError(false, "Not found"));

        return CustomResult<User>.Success(user);
    }

    public async Task<CustomResult> UpdateUserAsync(int UserId, string AspId)
    {
        var currentUser = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == UserId);

        currentUser.AspId = AspId;

        await UpdateAsync(currentUser);

        return CustomResult.Success();
    }
}
