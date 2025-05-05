using System.ComponentModel.DataAnnotations;

namespace Application.Aggregates.UserAuthAggregate;

public sealed record ForgotPasswordRequest
{
    [StringLength(50, MinimumLength = 5,
    ErrorMessage = "{0} should be minimum 5 characters and a maximum of 50 characters")]
    [DataType(DataType.Text)]
    public required string Username { get; set; }
}
