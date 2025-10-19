using Domain.Enums;
using ServiceLayer.FileJobs;
using ServiceLayer.Users;

namespace WorkerServiceProcess
{
    public class Worker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<Worker> _logger;


        public Worker(IServiceProvider serviceProvider, ILogger<Worker> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {

                try
                {
                    // Create a scope each iteration (or per job) so we can resolve scoped services safely.
                    using var scope = _serviceProvider.CreateScope();

                    // Resolve any scoped services you need from the scope.
                    // Example: IFileJobService and IUserService are registered as scoped in Program.cs
                    var fileJobService = scope.ServiceProvider.GetRequiredService<IFileJobService>();
                    var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

                    var userList = await userService.GetUsers(true, Domain.Enums.UserType.TaskUser);

                    // If you don't have a specific job to run every loop, at minimum await a small delay.
                    var fileJobList = await fileJobService.GetFileJobListRunning(stoppingToken);

                    if (fileJobList != null)
                    {
                        _logger.LogInformation("ProcessFileJob started, job count: {number}", fileJobList.Count);
                    }

                    Random random = new Random();


                    foreach (var fileJobId in fileJobList!)
                    {
                        try
                        {
                            bool hasMore = true;

                            while (hasMore)
                            {
                                var fileUploadList = await fileJobService.GetFileJobUploadList(fileJobId, stoppingToken);

                                if (fileUploadList.Any())
                                {
                                   
                                    foreach (var row in fileUploadList)
                                    {
                                        var rndNumber = random.Next(2, 5);
                                        //TODO testing performance
                                        await Task.Delay(1000 * rndNumber);

                                        if (String.IsNullOrEmpty(row.AssignedTo))
                                        {
                                            row.FileRowType = FileRowStatus.Processed;
                                            row.AssignedToId = null;
                                        }
                                        else
                                        {
                                            var userId = userList.Where(uu => uu.FullName == row.AssignedTo)
                                                                               .Select(uu => uu.Id)
                                                                               .FirstOrDefault();

                                            if (userId == 0)
                                            {
                                                row.FileRowType = FileRowStatus.ProcessIssue;
                                                row.ErrorMessage = $"AssignedTo user '{row.AssignedTo}' not found.";
                                            }
                                            else
                                            {
                                                row.FileRowType = FileRowStatus.Processed;
                                                row.AssignedToId = userId;
                                            }
                                        }

                                    }

                                    //update range
                                    var updatedCount = await fileJobService.UpdateFileJobUploadRangeAsync(fileUploadList, stoppingToken);

                                    _logger.LogInformation("Updated count: {number}", updatedCount);
                                }
                                else
                                {
                                    hasMore = false;
                                }
                            }

                           await fileJobService.CompleteFileJobAsync(fileJobId, stoppingToken);
                            await fileJobService.MoveToLiveAsync(fileJobId, stoppingToken);

                            _logger.LogInformation("ProcessFileJob completed for FileJobId: {fileJobId}", fileJobId);

                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing FileJobId: {fileJobId}", fileJobId);
                        }
                    }



                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // graceful shutdown
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled exception in Worker loop.");
                }

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);


                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // Graceful shutdown
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing file jobs.");
                }

               
            }
        }
    }
}
