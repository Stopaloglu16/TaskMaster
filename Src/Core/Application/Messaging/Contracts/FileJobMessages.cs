namespace Application.Messaging.Contracts;

// The FileJob import saga. Commands go orchestrator -> participant; events come back the other way
// and are what advance the saga's state. Every record carries the FileJobId so a participant can do
// its work without loading the saga row.

// --- Step 1: validate the uploaded rows -------------------------------------
public record ValidateFile(Guid MessageId, Guid CorrelationId, int FileJobId) : IMessage;

public record FileValidated(Guid MessageId, Guid CorrelationId, int FileJobId, int ValidCount, int InvalidCount) : IMessage;

public record FileValidationFailed(Guid MessageId, Guid CorrelationId, int FileJobId, string Reason) : IMessage;

// --- Step 2: resolve assignees and mark rows processed -----------------------
public record ProcessRows(Guid MessageId, Guid CorrelationId, int FileJobId) : IMessage;

public record RowsProcessed(Guid MessageId, Guid CorrelationId, int FileJobId, int ProcessedCount) : IMessage;

public record RowsProcessingFailed(Guid MessageId, Guid CorrelationId, int FileJobId, string Reason) : IMessage;

// --- Step 3: promote the processed rows into TaskLists/TaskItems -------------
public record PromoteToLive(Guid MessageId, Guid CorrelationId, int FileJobId) : IMessage;

public record PromotedToLive(Guid MessageId, Guid CorrelationId, int FileJobId, int TaskListCount) : IMessage;

public record PromotionFailed(Guid MessageId, Guid CorrelationId, int FileJobId, string Reason) : IMessage;

// --- Compensation: undo a partial promotion ---------------------------------
public record RollbackPromotion(Guid MessageId, Guid CorrelationId, int FileJobId) : IMessage;

public record PromotionRolledBack(Guid MessageId, Guid CorrelationId, int FileJobId) : IMessage;
