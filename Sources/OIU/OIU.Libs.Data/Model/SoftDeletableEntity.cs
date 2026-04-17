using UnlimitSoft.Data;

namespace OIU.Libs.Data.Model;


/// <summary>
/// 
/// </summary>
public abstract class SoftDeletableEntity : Entity<long>, ISoftDeletable
{
    /// <inheritdoc />
    public bool IsDeleted { get; set; }
    /// <inheritdoc />
    public DateTime? Deleted { get; set; }
}
