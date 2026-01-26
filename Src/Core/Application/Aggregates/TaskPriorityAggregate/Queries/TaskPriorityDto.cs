namespace Application.Aggregates.TaskPriorityAggregate.Queries;

public record TaskPriorityDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Color { get; set; }
}
