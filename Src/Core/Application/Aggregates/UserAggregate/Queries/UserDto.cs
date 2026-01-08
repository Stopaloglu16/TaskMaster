using Domain.Enums;

namespace Application.Aggregates.UserAggregate.Queries;

public record UserDto
{
    public int Id { get; set; }
    public string? AspId { get; set; }
    public string FullName { get; set; }

    public string UserEmail { get; set; }

    public UserType UserType { get; set; }
}
