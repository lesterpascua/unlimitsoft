using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoftUnlimit.Cloud.Partner.Data.Model;
using SoftUnlimit.Data.EntityFramework.Utility;

namespace SoftUnlimit.Cloud.Partner.Data.Configuration
{
    public static class Utility
    {
        public static void ConfigurePending<TEntity>(EntityTypeBuilder<TEntity> builder, string name) where TEntity : Pending
        {
            builder.ToTable(name);
            builder.HasKey(k => k.Id);

            builder.Property(p => p.EventId).HasComment("Identifier of the event");
            builder.Property(p => p.SourceId).HasComment("Primary key for the entity is unique for all the system.");
            builder.Property(p => p.Version).HasComment("Event version number alwais is incremental for the same SourceId.");
            builder.Property(p => p.ServiceId).HasComment("Service identifier.");
            builder.Property(p => p.WorkerId).HasMaxLength(64).HasComment("Worker identifier where the event was generate.");
            builder.Property(p => p.CorrelationId).HasMaxLength(64).HasComment("Correlation of the event, indicate the trace were the event was generate.");
            builder.Property(p => p.Created).HasComment("Event create date.").HasConversion(DateTimeUtcConverter.Instance);
            builder.Property(p => p.Name).IsRequired().HasMaxLength(255).HasComment("Name of the event.");
            builder.Property(p => p.Body).HasComment("Body of the event serialized as json.");
            builder.Property(p => p.IdentityId).HasComment("Identity owner of the event.");
            builder.Property(p => p.PartnerId).HasComment("Partner identifier where the event comming from (if null is internal system).");
            builder.Property(p => p.Retry).HasComment("Retry attempt for this event.");
            builder.Property(p => p.Scheduler).HasComment("Scheduler time popone this event.").HasConversion(DateTimeUtcConverter.Instance); ;

            builder.HasIndex(i => i.Created).IsUnique(false);
            builder.HasIndex(i => i.Name).IsUnique(false);
            builder.HasIndex(i => i.EventId).IsUnique(false);
            builder.HasIndex(i => i.SourceId).IsUnique(false);
            builder.HasIndex(i => i.CorrelationId).IsUnique(false);
        }
        public static void ConfigureComplete<TEntity>(EntityTypeBuilder<TEntity> builder, string name) where TEntity : Complete
        {
            builder.ToTable(name);
            builder.HasKey(k => k.Id);

            builder.Property(p => p.EventId).HasComment("Identifier of the event");
            builder.Property(p => p.SourceId).HasComment("Primary key for the entity is unique for all the system.");
            builder.Property(p => p.Version).HasComment("Event version number alwais is incremental for the same SourceId.");
            builder.Property(p => p.ServiceId).HasComment("Service identifier.");
            builder.Property(p => p.WorkerId).HasMaxLength(64).HasComment("Worker identifier where the event was generate.");
            builder.Property(p => p.CorrelationId).HasMaxLength(64).HasComment("Correlation of the event, indicate the trace were the event was generate.");
            builder.Property(p => p.Created).HasComment("Event create date.").HasConversion(DateTimeUtcConverter.Instance);
            builder.Property(p => p.Name).IsRequired().HasMaxLength(255).HasComment("Name of the event.");
            builder.Property(p => p.Body).HasComment("Body of the event serialized as json.");
            builder.Property(p => p.IdentityId).HasComment("Identity owner of the event.");
            builder.Property(p => p.PartnerId).HasComment("Partner identifier where the event comming from (if null is internal system).");
            builder.Property(p => p.Retry).HasComment("Retry attempt for this event.");
            builder.Property(p => p.Completed).HasComment("Date where the event was process complete.").HasConversion(DateTimeUtcConverter.Instance);

            builder.HasIndex(i => i.Created).IsUnique(false);
            builder.HasIndex(i => i.Name).IsUnique(false);
            builder.HasIndex(i => i.EventId).IsUnique(false);
            builder.HasIndex(i => i.SourceId).IsUnique(false);
            builder.HasIndex(i => i.CorrelationId).IsUnique(false);
        }
    }
}
