using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Config;

public class FileJobConfiguration : IEntityTypeConfiguration<FileJob>
{
    public void Configure(EntityTypeBuilder<FileJob> builder)
    {
        // The saga instance id every message correlates on.
        builder.HasIndex(fileJob => fileJob.CorrelationId).IsUnique();

        // Stored as text rather than an int so the saga's state is readable straight from the table.
        builder.Property(fileJob => fileJob.State)
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsRequired();

        // The compare-and-swap that makes concurrent replies for one job safe: the loser gets a
        // DbUpdateConcurrencyException, which MessageDispatcher turns into a retryable Failed.
        builder.Property(fileJob => fileJob.Version).IsConcurrencyToken();

        builder.Property(fileJob => fileJob.FailureReason).HasMaxLength(1000);
    }
}
