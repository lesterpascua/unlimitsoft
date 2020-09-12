using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoftUnlimit.CQRS.EventSourcing.Binary;
using System;
using System.Collections.Generic;
using System.Text;

namespace App.Manual.Tests.CQRS.Configuration
{
    public class VersionedEventEntityTypeBuilder : _EntityTypeBuilder<BinaryVersionedEventPayload>
    {
        public override void Configure(EntityTypeBuilder<BinaryVersionedEventPayload> builder)
        {
            builder.HasKey(k => new { k.SourceID, k.Version });

            builder.Property(p => p.CreatorID).IsRequired().HasMaxLength(36);     // Guid
            builder.Property(p => p.SourceID).IsRequired().HasMaxLength(36);      // Guid
            builder.Property(p => p.ServiceID);
            builder.Property(p => p.WorkerID);
            builder.Property(p => p.EventName).HasMaxLength(255).IsRequired();
            builder.Property(p => p.CreatorName).HasMaxLength(255).IsRequired();
            builder.Property(p => p.RawData).IsRequired();

            builder.HasIndex(i => i.SourceID).IsUnique(false);
            builder.HasIndex(i => i.EventName).IsUnique(false);
            builder.HasIndex(i => i.CreatorName).IsUnique(false);
        }
    }
}
