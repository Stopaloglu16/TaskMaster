namespace WebApi.Notification
{
    public class WorkItem
    {
        public string ProcessId { get; set; } = Guid.NewGuid().ToString();
        public string ConnectionId { get; set; } = default!;
        public DateTime EnqueuedAt { get; set; } = DateTime.UtcNow;
    }
}
