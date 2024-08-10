using System.ComponentModel.DataAnnotations;

namespace Application.Aggregates.UserAggregate.Commands;

public record UpdateUserRequest
{
    public int Id { get; set; }

    [StringLength(50)]
    public required string FullName { get; set; }

    [StringLength(50)]
    public required string UserEmail { get; set; }
}
