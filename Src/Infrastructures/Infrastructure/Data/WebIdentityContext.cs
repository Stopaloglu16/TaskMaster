using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class WebIdentityContext : IdentityDbContext
{
    public WebIdentityContext(DbContextOptions<WebIdentityContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        SeedAdminUser(builder);

        //if (!Database.IsSqlite())
        //{
        //    SeedAdminUser(builder);
        //}
    }


    private void SeedAdminUser(ModelBuilder builder)
    {

        const string seedPassword = "SuperStrongPassword+123";

        var hasher = new PasswordHasher<IdentityUser>();

        foreach (var userType in Enum.GetValues(typeof(UserType)))
        {
            string seedUserName = $"{userType}@hotmail.co.uk";

            builder.Entity<IdentityUser>().HasData(new IdentityUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = userType.ToString(),
                NormalizedUserName = seedUserName.ToUpper(),
                Email = seedUserName,
                NormalizedEmail = seedUserName.ToUpper(),
                EmailConfirmed = true,
                PasswordHash = hasher.HashPassword(null, seedPassword)
            });
        }

    }

}