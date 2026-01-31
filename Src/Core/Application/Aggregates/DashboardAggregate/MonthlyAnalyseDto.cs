namespace Application.Aggregates.DashboardAggregate;


public record MonthlyAnalyseDto
{
    public int CompletedInTime { get; set; }
    public int CompletedLate { get; set; }
    public int NotCompleted { get; set; }
    public required string MonthName { get; set; }
}
