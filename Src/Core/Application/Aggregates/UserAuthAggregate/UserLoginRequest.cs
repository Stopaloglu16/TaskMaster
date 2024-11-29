using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Application.Aggregates.UserAuthAggregate;

[NotMapped]
public record UserLoginRequest
{
    [StringLength(50)]
    public required string Username { get; set; }

    [DataType(DataType.Password)]
    public required string Password { get; set; }
}
