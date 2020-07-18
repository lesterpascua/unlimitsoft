using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoftUnlimit.CQRS.EventSourcing;
using SoftUnlimit.CQRS.Test.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.CQRS.Test.Configuration
{
    public class EventSourcedPayloadEntityTypeBuilder : _EntityTypeBuilder<VersionedEventPayload>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        public override void Configure(EntityTypeBuilder<VersionedEventPayload> builder)
        {
            base.Configure(builder);

            builder.HasKey(k => new { k.SourceID, k.Version });

            builder.Property(p => p.EventType).IsRequired().HasMaxLength(256);
        }
    }
}
