using UnlimitSoft.Data;

namespace OIU.Libs.Data.Model;


/// <summary>
/// 
/// </summary>
public abstract class SoftDeletableUUidEntity : Entity<Guid>, ISoftDeletable
{
    /// <inheritdoc />
    public bool IsDeleted { get; set; }
    /// <inheritdoc />
    public DateTime? Deleted { get; set; }
}
