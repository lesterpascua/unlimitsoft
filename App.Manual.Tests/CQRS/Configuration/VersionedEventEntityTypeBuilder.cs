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
            builder.HasKey(k => new { k.SourceId, k.Version });

            builder.Property(p => p.CreatorId).IsRequired().HasMaxLength(36);     // Guid
            builder.Property(p => p.SourceId).IsRequired().HasMaxLength(36);      // Guid
            builder.Property(p => p.ServiceId);
            builder.Property(p => p.WorkerId).IsRequired().HasMaxLength(64);
            builder.Property(p => p.EventName).HasMaxLength(255).IsRequired();
            builder.Property(p => p.CreatorName).HasMaxLength(255).IsRequired();
            builder.Property(p => p.Payload).IsRequired();

            builder.HasIndex(i => i.SourceId).IsUnique(false);
            builder.HasIndex(i => i.EventName).IsUnique(false);
            builder.HasIndex(i => i.CreatorName).IsUnique(false);
        }
    }
}
