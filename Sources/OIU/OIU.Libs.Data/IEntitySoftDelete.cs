using UnlimitSoft.Data;

namespace OIU.Libs.Data;


/// <summary>
/// 
/// </summary>
public interface IEntitySoftDelete : IEntity
{
    /// <summary>
    /// Gets or sets a value indicating whether the entity is deleted.
    /// </summary>
    bool IsDeleted { get; set; }
    /// <summary>
    /// Gets or sets the date and time when the entity was deleted.
    /// </summary>
    DateTime? Deleted { get; set; }
}
/// <summary>
/// 
/// </summary>
public static class ISoftDeletableExtensions
{
    /// <summary>
    /// Marks the entity as deleted.
    /// </summary>
    public static void Delete(this IEntitySoftDelete entity, DateTime utcNow)
    {
        entity.IsDeleted = true;
        entity.Deleted = utcNow;
    }
}