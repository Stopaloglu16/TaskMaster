using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data;

public static class SeedData
{
    /// <summary>
    /// The Keycloak ids of the seeded realm users, verbatim from
    /// <c>TaskMaster.AppHost/Keycloak/taskmaster-realm.json</c>. These become the <c>sub</c> claim of
    /// every token those users are issued, and therefore the value <c>ICurrentUserService.UserId</c>
    /// stamps into audit columns — so the domain rows must be linked by exactly these strings. Change
    /// one here without changing the realm and the seeded users silently stop owning their data.
    /// </summary>
    private static readonly IReadOnlyDictionary<UserType, string> KeycloakUserIds = new Dictionary<UserType, string>
    {
        [UserType.AdminUser] = "11111111-1111-1111-1111-111111111111",
        [UserType.TaskUser] = "22222222-2222-2222-2222-222222222222",
        [UserType.ReadOnly] = "33333333-3333-3333-3333-333333333333",
    };

    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;

        var appDb = provider.GetRequiredService<ApplicationDbContext>();
        var logger = provider.GetService<ILoggerFactory>()?.CreateLogger(nameof(SeedData));

        try
        {
            // Roles, credentials and the accounts themselves live in the Keycloak realm now. All
            // that remains here is the domain User row each token's `sub` resolves to.
            foreach (UserType userType in Enum.GetValues(typeof(UserType)))
            {
                if (!KeycloakUserIds.TryGetValue(userType, out var aspId))
                {
                    logger?.LogWarning("No Keycloak id seeded for {UserType}; skipping.", userType);
                    continue;
                }

                var email = $"{userType}@hotmail.co.uk";

                var existing = await appDb.Users.FirstOrDefaultAsync(u => u.AspId == aspId || u.UserEmail == email);

                if (existing is not null)
                {
                    // Re-link rows seeded before the Keycloak cut-over, which still carry the old
                    // ASP.NET Identity GUID. Without this, a database that predates the migration
                    // authenticates fine and then fails with "Not registered user", because the
                    // token's `sub` matches no AspId.
                    if (existing.AspId != aspId)
                    {
                        logger?.LogInformation("Re-linking {Email} from AspId {Old} to Keycloak id {New}.", email, existing.AspId, aspId);
                        existing.AspId = aspId;
                    }

                    continue;
                }

                var domainUser = new Domain.Entities.User
                {
                    FullName = $"{userType} user",
                    UserEmail = email,
                    UserTypeId = userType,
                    AspId = aspId,
                    RegisterTokenExpieryTime = DateTime.UtcNow.AddDays(1),
                    RefreshTokenExpiryTime = DateTime.UtcNow
                };

                await appDb.Users.AddAsync(domainUser);
            }

            await appDb.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }
}
