using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using Application.Messaging;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Infrastructure.Messaging;

public enum DispatchStatus
{
    /// <summary>Handler ran and its transaction committed. Ack.</summary>
    Handled,

    /// <summary>This consumer group has already processed this message id. Ack, do nothing.</summary>
    Duplicate,

    /// <summary>
    /// Unknown type, unregistered handler, or undeserializable payload. Retrying cannot help, so this
    /// goes straight to the dead-letter queue rather than round-tripping the retry ladder.
    /// </summary>
    Unroutable,

    /// <summary>Handler threw, or the commit lost a concurrency race. Retry with backoff.</summary>
    Failed,
}

public readonly record struct DispatchOutcome(DispatchStatus Status, string? Error = null);

/// <summary>
/// The consuming half of the inbox pattern, and the only place a handler's transaction is managed.
/// For one delivery it: dedupes against the inbox, then in a single transaction inserts the inbox
/// row, runs the handler, and commits whatever the handler changed plus any messages it enqueued.
/// <para>
/// It is transport-agnostic on purpose — both the RabbitMQ consumer and the in-process transport
/// funnel through here, so delivery semantics are decided by the caller (ack / retry / dead-letter)
/// and correctness of the database side is decided here.
/// </para>
/// </summary>
public sealed class MessageDispatcher<TDbContext>(
    IServiceScopeFactory scopeFactory,
    MessageTypeRegistry registry,
    ILogger<MessageDispatcher<TDbContext>> logger)
    where TDbContext : OutboxDbContext
{
    private static readonly ConcurrentDictionary<Type, MethodInfo> HandleMethods = new();

    /// <summary>Postgres unique-violation SQLSTATE.</summary>
    private const string UniqueViolation = "23505";

    public async Task<DispatchOutcome> DispatchAsync(
        string consumer,
        string typeName,
        string payload,
        CancellationToken cancellationToken)
    {
        var clrType = registry.Resolve(typeName);
        if (clrType is null)
        {
            return new DispatchOutcome(DispatchStatus.Unroutable, $"No CLR type registered for '{typeName}'.");
        }

        IMessage message;
        try
        {
            message = (IMessage)JsonSerializer.Deserialize(payload, clrType)!;
        }
        catch (JsonException ex)
        {
            return new DispatchOutcome(DispatchStatus.Unroutable, $"Undeserializable {typeName}: {ex.Message}");
        }

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TDbContext>();

        // Cheap pre-check. The authoritative dedupe is the inbox primary key inside the transaction
        // below — this one only saves the work when the duplicate is not concurrent.
        if (await db.InboxMessages.AnyAsync(
                x => x.MessageId == message.MessageId && x.Consumer == consumer, cancellationToken))
        {
            return new DispatchOutcome(DispatchStatus.Duplicate);
        }

        var handlerType = typeof(IMessageHandler<>).MakeGenericType(clrType);
        var handler = scope.ServiceProvider.GetService(handlerType);
        if (handler is null)
        {
            return new DispatchOutcome(DispatchStatus.Unroutable, $"No handler registered for '{typeName}'.");
        }

        try
        {
            await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);

            db.InboxMessages.Add(new InboxMessage
            {
                MessageId = message.MessageId,
                Consumer = consumer,
                Type = typeName,
                ReceivedOn = DateTime.UtcNow,
            });

            var handle = HandleMethods.GetOrAdd(
                clrType,
                static (_, ht) => ht.GetMethod(nameof(IMessageHandler<IMessage>.HandleAsync))!,
                handlerType);

            await (Task)handle.Invoke(handler, [message, cancellationToken])!;

            // One commit for: inbox row + business change + any outbox rows the handler enqueued.
            await db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            logger.LogInformation("{Consumer} handled {Type} ({Id}).", consumer, typeName, message.MessageId);
            return new DispatchOutcome(DispatchStatus.Handled);
        }
        catch (DbUpdateException ex) when (IsInboxUniqueViolation(ex))
        {
            // The same delivery was processed concurrently and the other transaction won the insert.
            // Its work is committed, so this one is a duplicate rather than a failure.
            logger.LogInformation(
                "{Consumer} lost the inbox race for {Type} ({Id}); treating as duplicate.",
                consumer, typeName, message.MessageId);
            return new DispatchOutcome(DispatchStatus.Duplicate);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Two messages touched the same saga row at once (xmin token). Genuinely transient.
            logger.LogWarning(
                "{Consumer} hit a concurrency conflict on {Type} ({Id}); will retry.",
                consumer, typeName, message.MessageId);
            return new DispatchOutcome(DispatchStatus.Failed, "Optimistic concurrency conflict.");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "{Consumer} failed to handle {Type} ({Id}).", consumer, typeName, message.MessageId);
            return new DispatchOutcome(DispatchStatus.Failed, ex.Message);
        }
    }

    private static bool IsInboxUniqueViolation(DbUpdateException ex) =>
        ex.InnerException is PostgresException { SqlState: UniqueViolation } pg &&
        pg.TableName == OutboxDbContext.InboxTable;
}
