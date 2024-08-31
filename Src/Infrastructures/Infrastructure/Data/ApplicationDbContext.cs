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
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }


    public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
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

}
