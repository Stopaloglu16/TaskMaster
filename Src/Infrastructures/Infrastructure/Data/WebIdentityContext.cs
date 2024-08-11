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
        const string adminUserName = "taskmaster@hotmail.co.uk";
        const string adminPassword = "SuperStrongPassword+123";

        var hasher = new PasswordHasher<IdentityUser>();

        builder.Entity<IdentityUser>().HasData(new IdentityUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = adminUserName,
            NormalizedUserName = adminUserName.ToUpper(),
            Email = adminUserName,
            NormalizedEmail = adminUserName.ToUpper(),
            EmailConfirmed = true,
            PasswordHash = hasher.HashPassword(null, adminPassword)
        });
    }

}