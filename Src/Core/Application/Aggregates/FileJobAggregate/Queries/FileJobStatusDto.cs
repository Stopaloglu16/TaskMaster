namespace Application.Aggregates.FileJobAggregate.Queries
{
    public record FileJobStatusDto
    {
        public int Total { get; init; }
        public int Processed { get; init; }
    }
}
