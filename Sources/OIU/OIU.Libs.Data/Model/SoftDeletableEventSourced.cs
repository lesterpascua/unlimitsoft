using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnlimitSoft.CQRS.Data;

namespace OIU.Libs.Data.Model;


/// <summary>
/// 
/// </summary>
public abstract class SoftDeletableEventSourced : EventSourced, ISoftDeletable
{
    /// <inheritdoc />
    public bool IsDeleted { get; set; }
    /// <inheritdoc />
    public DateTime? Deleted { get; set; }

    /// <summary>
    /// Allow to set the server type for the entity. This will be used to configure the migration according with the server.
    /// </summary>
    public static ServerType ServerType { get; set; }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="builder"></param>
    /// <param name="addIndex">Add index to allow fast query</param>
    /// <param name="addFilter">Add filter for the entities</param>
    public static void EntityTypeBuilder<T>(EntityTypeBuilder<T> builder, bool addIndex = true, bool addFilter = true) where T : class, ISoftDeletable
    {
        const string IsDeletedColumnName = "is_deleted";

        builder.Property(p => p.IsDeleted).HasColumnOrder(3).HasColumnName(IsDeletedColumnName).HasComment("Gets or sets a value indicating whether the entity is deleted");
        builder.Property(p => p.Deleted).HasColumnOrder(4).HasComment("Gets or sets the date and time when the entity was deleted");

        //builder.Property(p => p.Version).HasColumnOrder(1_000_000_000).HasComment("Gets the entity's version. As the entity is being updated and events being generated, the version is incremented");

        if (addIndex)
        {
            switch (ServerType)
            {
                case ServerType.SqlServer:
                    builder.HasIndex(x => x.IsDeleted).HasFilter($"{IsDeletedColumnName} = 1");
                    break;
                case ServerType.PostgreSQL:
                    builder.HasIndex(x => x.IsDeleted).HasFilter($"{IsDeletedColumnName} = true");
                    break;
            }
        }
        if (addFilter)
            builder.HasQueryFilter(x => !x.IsDeleted);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="builder"></param>
    /// <param name="addIndex">Add index to allow fast query</param>
    /// <param name="addFilter">Add filter for the entities</param>
    public static void EventSourcedEntityTypeBuilder<T>(EntityTypeBuilder<T> builder, bool addIndex = true, bool addFilter = true) where T : class, ISoftDeletable, IEventSourced
    {
        EntityTypeBuilder(builder, addIndex, addFilter);
        builder.Property(p => p.Version).HasColumnOrder(1_000_000_000).HasComment("Gets the entity's version. As the entity is being updated and events being generated, the version is incremented");
    }
}
