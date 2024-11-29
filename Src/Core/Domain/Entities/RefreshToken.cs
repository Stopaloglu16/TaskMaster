using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class RefreshToken
{

    [Key]
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Token { get; set; }
    public DateTime ExpiryDate { get; set; }

    public bool IsRevoked { get; set; }
    public bool IsUsed { get; set; }

    [ForeignKey(nameof(UserId))]
    [InverseProperty("RefreshTokens")]
    public virtual User User { get; set; }

}
