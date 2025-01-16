using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Infrastructure.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentUserService _currentUserService;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
                                ICurrentUserService currentUserService) : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<TaskList> TaskLists { get; set; }
    public DbSet<TaskItem> TaskItems { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        SeedAdminUser(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }


    public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (_currentUserService != null)
            {
                if (!String.IsNullOrEmpty(_currentUserService.UserId) && !String.IsNullOrEmpty(_currentUserService.UserName))
                {
                    var userId = _currentUserService.UserId;

                    foreach (var entry in ChangeTracker.Entries<BaseAuditableEntity<int>>())
                    {
                        switch (entry.State)
                        {
                            case EntityState.Added:
                                entry.Entity.CreatedBy = _currentUserService.UserName;
                                entry.Entity.Created = DateTime.Now;
                                break;
                            case EntityState.Modified:
                                entry.Entity.LastModifiedBy = _currentUserService.UserName;
                                entry.Entity.LastModified = DateTime.Now;
                                break;
                        }
                    }
                }
            }

            var result = await base.SaveChangesAsync(cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            throw new Exception($"SaveChangesAsync {ex.Message}");
        }
    }


    private void SeedAdminUser(ModelBuilder builder)
    {
        const string adminUserName = "taskmaster@hotmail.co.uk";


        builder.Entity<User>().HasData(new User
        {
            Id = 1,
            FullName = adminUserName,
            UserEmail = adminUserName,
            UserTypeId = Domain.Enums.UserType.AdminUser
        });
    }
}
