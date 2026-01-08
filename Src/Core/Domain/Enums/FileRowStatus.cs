namespace Domain.Enums
{

    public enum FileRowStatus
    {
        NewUpload = 0,
        ValidateIssue = 1,
        Validated = 2,
        Processed = 3,
        ProcessIssue = 4,
        MovedToLive = 5
    }
}
