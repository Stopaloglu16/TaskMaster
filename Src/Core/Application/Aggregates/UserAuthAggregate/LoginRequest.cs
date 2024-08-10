using System.ComponentModel.DataAnnotations;

namespace Application.Aggregates.UserAuthAggregate;

public record LoginRequest
{
    [StringLength(50)]
    public required string Username { get; init; }

    [DataType(DataType.Password)]
    public required string Password { get; set; }
}
