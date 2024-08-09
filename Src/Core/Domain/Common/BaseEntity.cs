using System.ComponentModel.DataAnnotations;

namespace Domain.Common;

public abstract class BaseEntity<T>
{
    [Key]
    [Required]
    public T Id { get; set; }

    /// <summary>
    /// 0 not deleted
    /// 1 deleted
    /// 2 Main deleted, but sub soft deleted
    /// </summary>
    public byte IsDeleted { get; set; } = 0;
}
