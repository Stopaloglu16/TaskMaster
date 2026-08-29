namespace Infrastructure.Messaging.RabbitMq;

/// <summary>
/// Naming and retry policy for the broker topology. Bound from the <c>Messaging:RabbitMq</c>
/// configuration section; every exchange and queue name is derived from <see cref="Exchange"/> so the
/// whole topology can be namespaced (per environment, per developer) by changing one value.
/// </summary>
public sealed class RabbitMqOptions
{
    public const string SectionName = "Messaging:RabbitMq";

    /// <summary>Root name. The primary exchange is a topic exchange with exactly this name.</summary>
    public string Exchange { get; set; } = "saga";

    /// <summary>
    /// The retry ladder. A failed delivery is parked in the queue for its attempt number and comes
    /// back after that many seconds; once the ladder is exhausted the message is dead-lettered.
    /// So a message gets <c>1 + RetryDelaysSeconds.Length</c> deliveries in total.
    /// </summary>
    public int[] RetryDelaysSeconds { get; set; } = [5, 30, 120];

    /// <summary>Number of rungs on the ladder.</summary>
    public int MaxRetries => RetryDelaysSeconds.Length;

    /// <summary>
    /// Direct exchange that retried messages land on when their delay expires. Each consumer queue is
    /// bound to it by its own name, which is how a retry goes back to only the group that failed
    /// instead of fanning out to every group again.
    /// </summary>
    public string RequeueExchange => $"{Exchange}.requeue";

    /// <summary>Direct exchange for messages that have exhausted the ladder, routed by group name.</summary>
    public string DeadLetterExchange => $"{Exchange}.dlx";

    /// <summary>
    /// One fanout exchange + queue per rung. A fanout (rather than one shared retry queue with
    /// per-message TTL) because the delay is a property of the rung: every message in a given queue
    /// has the same TTL, so they expire in arrival order and no message can be stuck behind a
    /// longer-lived one at the head of the queue.
    /// </summary>
    public string RetryExchange(int rung) => $"{Exchange}.retry.{RetryDelaysSeconds[rung]}s";

    /// <inheritdoc cref="RetryExchange"/>
    public string RetryQueue(int rung) => RetryExchange(rung);

    /// <summary>Where a group's poison messages are parked for a human to look at.</summary>
    public string DeadLetterQueue(string consumerGroup) => $"{consumerGroup}.dead";

    /// <summary>Header carrying how many times delivery has already failed.</summary>
    public const string AttemptHeader = "x-attempt";

    /// <summary>Header carrying why the message was dead-lettered.</summary>
    public const string ErrorHeader = "x-error";
}
