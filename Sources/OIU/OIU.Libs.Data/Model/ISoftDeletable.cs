using UnlimitSoft;
using UnlimitSoft.Data;

namespace OIU.Libs.Data.Model;


/// <summary>
/// Define an entity is soft-deletable.
/// </summary>
public interface ISoftDeletable : IEntity
{
    /// <summary>
    /// Gets or sets a value indicating whether the entity is deleted.
    /// </summary>
    bool IsDeleted { get; set; }
    /// <summary>
    /// Gets or sets the date and time when the entity was deleted
    /// </summary>
    DateTime? Deleted { get; set; }
}
/// <summary>
/// 
/// </summary>
public static class SoftDeletableExtensions
{
    /// <summary>
    /// Soft delete entity.
    /// </summary>
    /// <param name="this"></param>
    /// <param name="utcNow"></param>
    public static void Delete(this ISoftDeletable @this, DateTime? utcNow = null)
    {
        @this.IsDeleted = true;
        @this.Deleted = utcNow ?? SysClock.GetUtcNow();
    }
}