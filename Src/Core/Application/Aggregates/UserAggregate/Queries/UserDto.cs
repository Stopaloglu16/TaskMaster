namespace Application.Aggregates.UserAggregate.Queries;

public record UserDto
{
    public required string FullName { get; set; }

    public required string UserEmail { get; set; }

    public string UserType { get; set; }
}
