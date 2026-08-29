using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    /// <summary>
    /// A bulk import job. This row IS the saga instance: <see cref="State"/> is the saga's state and
    /// <see cref="Version"/> is the optimistic-concurrency token that turns two replies arriving for
    /// the same job into a compare-and-swap rather than a lost update.
    /// </summary>
    public class FileJob : BaseAuditableEntity<int>
    {
        /// <summary>
        /// Stable saga id, carried on every message. Deliberately separate from the int primary key
        /// so the correlation id does not depend on database-assigned identity.
        /// </summary>
        public Guid CorrelationId { get; set; } = Guid.NewGuid();

        public bool IsCompleted { get; set; } = false;

        public SagaState State { get; set; } = SagaState.NewUpload;

        /// <summary>Why the saga was cancelled, when it was.</summary>
        public string? FailureReason { get; set; }

        /// <summary>Bumped on every saga transition. Configured as a concurrency token.</summary>
        public int Version { get; set; }

        public virtual IList<FileJobUpload> FileJobUploads { get; private set; } = new List<FileJobUpload>();
    }
}
