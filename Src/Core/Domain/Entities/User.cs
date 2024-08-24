using Domain.Common;
using Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;


public class User : BaseAuditableEntity<int>
{
    [Column(TypeName = "varchar(100)")]
    public required string FullName { get; set; }

    [Column(TypeName = "varchar(250)")]
    public required string UserEmail { get; set; }

    public UserType UserTypeId { get; set; }

    public string? AspId { get; set; }

    public Guid RegisterToken { get; set; } = Guid.NewGuid();
    public DateTime RegisterTokenValid { get; set; }

    public virtual ICollection<TaskList>? TaskLists { get; set; } = new List<TaskList>();


    [InverseProperty(nameof(RefreshToken.User))]
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; }

}
