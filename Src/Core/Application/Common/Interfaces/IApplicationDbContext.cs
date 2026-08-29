using Application.Messaging;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interfaces;

public interface IApplicationDbContext
{
    public DbSet<TaskList> TaskLists { get; set; }
    public DbSet<TaskItem> TaskItems { get; set; }
    public DbSet<User> Users { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Stage a message to be published after this unit of work commits. The outbox row and the
    /// business change land in the same transaction — see <c>OutboxDbContext.Enqueue</c>.
    /// </summary>
    void Enqueue(IMessage message);
}
