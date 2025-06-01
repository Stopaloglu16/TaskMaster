using Microsoft.AspNetCore.SignalR;

namespace WebApi.Notification
{
    public class TaskProgressHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }


        //public class LongRunningJobWithNotification(ILogger<LongRunningJob> logger, IHubContext<NotificationHub> hubContext)
        //{
        //    public async Task ExecuteAsync(CancellationToken cancellationToken)
        //    {
        //        logger.LogInformation("Starting background job");

        //        await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);

        //        logger.LogInformation("Completed background job");

        //        await hubContext.Clients.All.SendAsync("ReceiveNotification", "Completed processing job");
        //    }
        //}

        public async Task JoinGroup(string requestId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, requestId);
        }

        //public override async Task OnConnectedAsync()
        //{
        //    var connectionId = Context.ConnectionId;
        //    Console.WriteLine($"Client connected: {connectionId}");
        //    await base.OnConnectedAsync();
        //}
    }

    //public class LongRunningJob(ILogger<LongRunningJob> logger)
    //{
    //    public async Task ExecuteAsync(CancellationToken cancellationToken)
    //    {
    //        logger.LogInformation("Starting background job");

    //        await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);

    //        logger.LogInformation("Completed background job");
    //    }
    //}
}
