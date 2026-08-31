using System.ComponentModel.DataAnnotations;

namespace Application.Aggregates.UserAuthAggregate;

/// <summary>
/// What the in-app reset page posts. The token is the <c>RegisterToken</c> the forgot-password
/// request minted, so the same invite-token machinery covers both flows.
/// </summary>
public record ResetPasswordRequest
{
    [Required(ErrorMessage = "{0} is required")]
    [StringLength(50, MinimumLength = 5,
    ErrorMessage = "{0} should be minimum 5 characters and a maximum of 50 characters")]
    [DataType(DataType.Text)]
    public string Username { get; set; }


    [Required(ErrorMessage = "{0} is required")]
    [StringLength(50, MinimumLength = 8,
    ErrorMessage = "{0} should be minimum 8 characters and a maximum of 50 characters")]
    [DataType(DataType.Password)]
    public string Password { get; set; }


    [Required(ErrorMessage = "{0} is required")]
    [StringLength(50, MinimumLength = 8,
    ErrorMessage = "{0} should be minimum 8 characters and a maximum of 50 characters")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "The passwords do not match")]
    public string ConfirmPassword { get; set; }


    public string TokenConfirm { get; set; }
}
