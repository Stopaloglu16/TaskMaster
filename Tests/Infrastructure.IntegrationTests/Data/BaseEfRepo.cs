using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Infrastructure.IntegrationTests.Data;


public abstract class BaseEfRepo
{
    protected ApplicationDbContext _dbContext;

    protected BaseEfRepo()
    {
        _dbContext = GetDbContext();
    }

    protected static ApplicationDbContext GetDbContext()
    {
        var builder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Use In-Memory DB.
        builder.UseInMemoryDatabase(Guid.NewGuid().ToString()).ConfigureWarnings(w =>
        {
            w.Ignore(InMemoryEventId.TransactionIgnoredWarning);
        });

        var dbContext = new ApplicationDbContext(builder.Options, null);

        dbContext.Database.EnsureCreated();

        return dbContext;
    }

    protected EfCoreRepository<TaskList, int> GetRepository()
    {
        return new EfCoreRepository<TaskList, int>(_dbContext);
    }
}
