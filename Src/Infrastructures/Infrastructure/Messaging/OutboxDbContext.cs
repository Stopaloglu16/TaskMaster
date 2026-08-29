using System.Text.Json;
using Application.Messaging;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Messaging;

/// <summary>
/// Base <see cref="DbContext"/> that gives the application an outbox and an inbox table.
/// <c>ApplicationDbContext</c> derives from this and adds the domain entities.
/// </summary>
public abstract class OutboxDbContext : DbContext
{
    /// <summary>Postgres schema the outbox/inbox tables live in.</summary>
    public const string Schema = "messaging";

    public const string OutboxTable = "outbox_messages";
    public const string InboxTable = "inbox_messages";

    protected OutboxDbContext(DbContextOptions options) : base(options) { }

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    /// <summary>
    /// Stage a message for publication. Call this from within your handler/endpoint and then
    /// <c>SaveChanges</c> once — the business change and this outbox row commit atomically, so the
    /// system can never end up having applied the change without sending the message (or vice versa).
    /// Never publish to the broker directly; <see cref="OutboxRelay{TDbContext}"/> does that.
    /// </summary>
    public void Enqueue(IMessage message)
    {
        OutboxMessages.Add(new OutboxMessage
        {
            Id = message.MessageId,
            Type = message.GetType().Name,
            Payload = JsonSerializer.Serialize(message, message.GetType()),
            CorrelationId = message.CorrelationId,
            OccurredOn = DateTime.UtcNow,
        });
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OutboxMessage>(b =>
        {
            b.ToTable(OutboxTable, Schema);

            // Sequence is the key so Npgsql gives it a plain identity column — that is what makes the
            // relay's ordering monotonic and its keyset paging stable.
            b.HasKey(x => x.Sequence);
            b.Property(x => x.Sequence).UseIdentityAlwaysColumn();

            b.HasIndex(x => x.Id).IsUnique();
            b.Property(x => x.Type).IsRequired();
            b.Property(x => x.Payload).IsRequired();

            // Partial index: the relay's hot query is "unprocessed, in sequence order", and once a
            // message is published its row is dead weight the index should not carry.
            b.HasIndex(x => x.Sequence)
                .HasFilter($"\"{nameof(OutboxMessage.ProcessedOn)}\" IS NULL")
                .HasDatabaseName("ix_outbox_pending");
        });

        modelBuilder.Entity<InboxMessage>(b =>
        {
            b.ToTable(InboxTable, Schema);
            b.HasKey(x => new { x.MessageId, x.Consumer });
            b.Property(x => x.Consumer).HasMaxLength(64);
            b.Property(x => x.Type).IsRequired();
        });
    }
}
