using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using Domain.Entities.SearchEntities;
using Domain.Enums;
using Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using TickerQ.EntityFrameworkCore.Configurations;

namespace Infrastructure.Data;

public class ApplicationDbContext : OutboxDbContext, IApplicationDbContext
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

    public DbSet<TaskPriority> TaskPriority { get; set; }

    public DbSet<FileJob> FileJobs { get; set; }
    public DbSet<FileJobUpload> FileJobUploads { get; set; }

    public DbSet<User> Users { get; set; }

    /// <summary>
    /// Advanced Search Tables
    /// </summary>
    /// <param name="builder"></param>

    public DbSet<AdvancedSearch> AdvancedSearches { get; set; }
    public DbSet<AdvancedSearchColumn> AdvancedSearchColumns { get; set; }
    public DbSet<AdvancedSearchJoin> AdvancedSearchJoins { get; set; }
    public DbSet<AdvancedSearchTable> AdvancedSearchTables { get; set; }
    public DbSet<ColumnType> ColumnTypes { get; set; }
    public DbSet<Operator> Operators { get; set; }



    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<TaskList>().HasQueryFilter(p => p.IsDeleted == 0);
        builder.Entity<TaskItem>().HasQueryFilter(p => p.IsDeleted == 0);
        builder.Entity<User>().HasQueryFilter(p => p.IsDeleted == 0);

        // The Sqlite branch that used to guard this is gone — Postgres is the only provider now,
        // and it supports schemas, so the search tables always get their own.
        builder.Entity<AdvancedSearch>().ToTable("AdvancedSearches", "search");
        builder.Entity<AdvancedSearchColumn>().ToTable("AdvancedSearchColumns", "search");
        builder.Entity<AdvancedSearchJoin>().ToTable("AdvancedSearchJoins", "search");
        builder.Entity<AdvancedSearchTable>().ToTable("AdvancedSearchTables", "search");
        builder.Entity<ColumnType>().ToTable("ColumnTypes", "search");
        builder.Entity<Operator>().ToTable("Operators", "search");

        // Configure the join table in the "search" schema
        builder.Entity<Operator>()
               .HasMany(e => e.ColumnTypes)
               .WithMany(e => e.Operators)
               .UsingEntity(j => j.ToTable("ColumnTypeOperatorMapping", "search"));

        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // No try/catch here on purpose. The messaging layer distinguishes DbUpdateException
        // (inbox unique violation -> duplicate delivery) from DbUpdateConcurrencyException
        // (lost saga race -> retry), so the exception type has to survive.
        if (_currentUserService != null)
        {
            if (!string.IsNullOrEmpty(_currentUserService.UserId) && !string.IsNullOrEmpty(_currentUserService.UserName))
            {
                foreach (var entry in ChangeTracker.Entries<BaseAuditableEntity<int>>())
                {
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            entry.Entity.CreatedBy = _currentUserService.UserName;
                            entry.Entity.Created = DateTime.UtcNow;
                            break;
                        case EntityState.Modified:
                            entry.Entity.LastModifiedBy = _currentUserService.UserName;
                            entry.Entity.LastModified = DateTime.UtcNow;
                            break;
                    }
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
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
