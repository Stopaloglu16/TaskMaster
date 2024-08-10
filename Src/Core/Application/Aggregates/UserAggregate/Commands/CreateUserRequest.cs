using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.Aggregates.UserAggregate.Commands;

public record CreateUserRequest
{

    [StringLength(50)]
    public required string FullName { get; set; }

    [StringLength(50)]
    public required string UserEmail { get; set; }

    public UserType UserType { get; init; }

}
