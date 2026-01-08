using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection;
using TickerQ.EntityFrameworkCore.Configurations;

namespace Infrastructure.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentUserService _currentUserService;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
                                ICurrentUserService currentUserService) : base(options)
    {
        _currentUserService = currentUserService;
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        
    }

    public DbSet<TaskList> TaskLists { get; set; }
    public DbSet<TaskItem> TaskItems { get; set; }

    public DbSet<FileJob> FileJobs { get; set; }
    public DbSet<FileJobUpload> FileJobUploads { get; set; }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<TaskList>().HasQueryFilter(p => p.IsDeleted == 0);
        builder.Entity<TaskItem>().HasQueryFilter(p => p.IsDeleted == 0);
        builder.Entity<User>().HasQueryFilter(p => p.IsDeleted == 0);

        base.OnModelCreating(builder);

        //builder.ApplyConfiguration(new TimeTickerConfigurations(  "ticker"));
        //builder.ApplyConfiguration(new CronTickerConfigurations("ticker"));
        //builder.ApplyConfiguration(new CronTickerOccurrenceConfigurations("ticker"));


        //SeedAdminUser(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_currentUserService != null)
            {
                if (!string.IsNullOrEmpty(_currentUserService.UserId) && !string.IsNullOrEmpty(_currentUserService.UserName))
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

    //private void SeedAdminUser(ModelBuilder builder)
    //{
    //    foreach (var userType in Enum.GetValues(typeof(UserType)))
    //    {
    //        builder.Entity<User>().HasData(new User
    //        {
    //            FullName = userType + " user",
    //            UserEmail = $"{userType}@hotmail.co.uk",
    //            UserTypeId = userType is UserType.AdminUser ? Domain.Enums.UserType.AdminUser :
    //                             userType is UserType.TaskUser ? Domain.Enums.UserType.TaskUser :
    //                             Domain.Enums.UserType.ReadOnly
    //        });
    //    }
    //}


}
