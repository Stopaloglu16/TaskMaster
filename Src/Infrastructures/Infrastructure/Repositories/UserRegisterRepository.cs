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

        // Compared case-insensitively: Keycloak lowercases every email it stores, while these rows
        // keep whatever an admin typed ("AdminUser@hotmail.co.uk"), and Postgres's = is case
        // sensitive — so a straight comparison loses the seeded users.
        var email = Username.ToLower();

        var user = await _dbContext.Users.AsNoTracking()
                                     .Where(u => u.UserEmail.ToLower() == email &&
                                                     u.RegisterToken == guid)
                                     .FirstOrDefaultAsync();
        if (user is null)
            return CustomResult<User>.Failure(new CustomError(false, "Not found"));

        return CustomResult<User>.Success(user);
    }

    public async Task<CustomResult<User>> GetUserByAsync(string Username)
    {
        var email = Username.ToLower();

        var user = await _dbContext.Users.AsNoTracking()
                                     .Where(u => u.UserEmail.ToLower() == email)
                                     .FirstOrDefaultAsync();
        if (user is null)
            return CustomResult<User>.Failure(new CustomError(false, "Not found"));

        return CustomResult<User>.Success(user);
    }

    // Both of these load the entity *tracked* and save through the context, rather than the
    // AsNoTracking + EfCoreRepository.UpdateAsync (which Attaches) they used to. Registration now
    // calls them one after the other in a single request scope, and attaching a second instance of
    // an already-tracked User throws "cannot be tracked because another instance with the same key
    // value is already being tracked".

    public async Task<CustomResult> ExpireRegisterTokenAsync(int UserId)
    {
        var currentUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == UserId);

        if (currentUser is null)
            return CustomResult.Failure("Not found");

        // A new GUID as well as a past expiry, so the emailed link stops matching outright.
        currentUser.RegisterToken = Guid.NewGuid();
        currentUser.RegisterTokenExpieryTime = DateTime.UtcNow.AddSeconds(-1);

        await _dbContext.SaveChangesAsync(CancellationToken.None);

        return CustomResult.Success();
    }

    public async Task<CustomResult> UpdateUserAsync(int UserId, string AspId)
    {
        var currentUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == UserId);

        if (currentUser is null)
            return CustomResult.Failure("Not found");

        currentUser.AspId = AspId;

        await _dbContext.SaveChangesAsync(CancellationToken.None);

        return CustomResult.Success();
    }
}
