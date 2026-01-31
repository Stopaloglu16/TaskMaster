namespace Application.Aggregates.TaskPriorityAggregate.Queries;

public record TaskPrioritySelect
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool Default { get; set; }
}
