using System.ComponentModel.DataAnnotations;

namespace Application.Aggregates.UserAuthAggregate
{
    public record ForgotPasswordRequest
    {
        [Required(ErrorMessage = "{0} is required")]
        [StringLength(50, MinimumLength = 5,
        ErrorMessage = "{0} should be minimum 5 characters and a maximum of 50 characters")]
        [DataType(DataType.Text)]
        public string Username { get; set; }
    }
}
