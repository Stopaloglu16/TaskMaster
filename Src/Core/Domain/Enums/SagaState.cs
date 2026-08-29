namespace Domain.Enums
{
    /// <summary>
    /// The FileJob import saga's state. Replaces the old FileJobType, whose Validated member was
    /// never written by any code.
    /// </summary>
    public enum SagaState
    {
        /// <summary>Rows uploaded; the saga has not been started yet.</summary>
        NewUpload = 0,

        /// <summary>ValidateFile has been enqueued; the saga is running.</summary>
        Started = 1,

        /// <summary>Rows validated; waiting on ProcessRows.</summary>
        Validated = 2,

        /// <summary>Assignees resolved; waiting on PromoteToLive.</summary>
        Processed = 3,

        /// <summary>Rows promoted into TaskLists/TaskItems. Terminal, success.</summary>
        Completed = 4,

        /// <summary>A step failed after work was done; compensation is running.</summary>
        Compensating = 5,

        /// <summary>Terminal, failure. Any partial promotion has been rolled back.</summary>
        Cancelled = 6,
    }
}
