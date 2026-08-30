namespace WebApi.RabbitMq
{
    /// <summary>
    /// One place for the queue name, so publisher and consumer cannot drift apart again.
    /// </summary>
    public static class RabbitQueues
    {
        public const string TaskListBulk = "catalogEvents";
        public const bool Durable = true;
    }
}
