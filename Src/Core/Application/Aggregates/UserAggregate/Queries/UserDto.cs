using Domain.Enums;

namespace Application.Aggregates.UserAggregate.Queries;

public record UserDto
{
    public int Id { get; set; }
    public string? AspId { get; init; }
    public required string FullName { get; init; }

    public string? UserEmail { get; init; }

    public UserType UserType { get; init; }
}
