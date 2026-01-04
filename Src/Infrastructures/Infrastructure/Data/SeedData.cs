using System;
using System.Linq;
using System.Threading.Tasks;
using Domain.Enums;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services, string? seedPassword = null)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;

        var userManager = provider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
        var appDb = provider.GetRequiredService<ApplicationDbContext>();
        var logger = provider.GetService<ILoggerFactory>()?.CreateLogger(nameof(SeedData));

        seedPassword ??= "SuperStrongPassword+123";

        try
        {
            // Ensure roles for each UserType
            foreach (UserType userType in Enum.GetValues(typeof(UserType)))
            {
                var roleName = userType.ToString();
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                    if (!roleResult.Succeeded)
                    {
                        logger?.LogWarning("Failed to create role {Role}: {Errors}", roleName, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                    }
                }
            }

            // Create identity users and corresponding domain users
            foreach (UserType userType in Enum.GetValues(typeof(UserType)))
            {
                var email = $"{userType}@hotmail.co.uk";
                var identityUser = await userManager.FindByEmailAsync(email);
                if (identityUser == null)
                {
                    identityUser = new IdentityUser
                    {
                        UserName = userType.ToString(),
                        Email = email,
                        EmailConfirmed = true
                    };

                    var createResult = await userManager.CreateAsync(identityUser, seedPassword);
                    if (!createResult.Succeeded)
                    {
                        logger?.LogError("Failed to create identity user {Email}: {Errors}", email, string.Join(", ", createResult.Errors.Select(e => e.Description)));
                        continue;
                    }

                    await userManager.AddToRoleAsync(identityUser, userType.ToString());
                }

                // Ensure domain user exists and is linked with AspId
                var domainUserExists = await appDb.Users.AnyAsync(u => u.AspId == identityUser.Id || u.UserEmail == email);
                if (!domainUserExists)
                {
                    var domainUser = new Domain.Entities.User
                    {
                        FullName = $"{userType} user",
                        UserEmail = email,
                        UserTypeId = userType,
                        AspId = identityUser.Id,
                        RegisterTokenExpieryTime = DateTime.UtcNow.AddDays(1),
                        RefreshTokenExpiryTime = DateTime.UtcNow
                    };

                    await appDb.Users.AddAsync(domainUser);
                }
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