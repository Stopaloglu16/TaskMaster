using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace WebApi.Services;


public class JobProcessWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public JobProcessWorker(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var fileJobs = await db.FileJobs
                .Where(fj => fj.FileJobType == FileJobType.Running)
                .ToListAsync(stoppingToken);

            foreach (var job in fileJobs)
            {

                var batch = await db.FileJobUploads
                .Where(r => r.Id == job.Id && r.FileRowType  == FileRowStatus.Validated)
                .OrderBy(r => r.Id)
                .Take(10)
                .ToListAsync(stoppingToken);

                if (batch.Any())
                {
                    foreach (var row in batch)
                    {
                        row.FileRowType = FileRowStatus.Processed;
                        // your logic here (e.g., API call, calculation)
                        //row.Result = $"Processed: {row.Data}";
                        //row.Status = "Completed";
                    }
                    await db.SaveChangesAsync(stoppingToken);
                }

                job.FileJobType = FileJobType.MovedToLive;

                await db.SaveChangesAsync(stoppingToken);
            }


            await Task.Delay(2000, stoppingToken); // check every 2 sec
        }
    }
}
