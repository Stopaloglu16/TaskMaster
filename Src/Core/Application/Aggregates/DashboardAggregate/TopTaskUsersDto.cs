namespace Application.Aggregates.DashboardAggregate;

public record TopTaskUsersDto
{
    public string Username { get; set; } = string.Empty; 
    public int TaskCount { get; set; } = 0;
}
