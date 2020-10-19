using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoftUnlimit.CQRS.EventSourcing.Binary;
using SoftUnlimit.CQRS.EventSourcing.Json;
using SoftUnlimit.Data.EntityFramework.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace App.Manual.Tests.CQRS.Configuration
{
    public class VersionedEventEntityTypeBuilder : _EntityTypeBuilder<JsonVersionedEventPayload>
    {
        public override void Configure(EntityTypeBuilder<JsonVersionedEventPayload> builder) 
            => EntityBuilderUtility.ConfigureVersionedEvent<JsonVersionedEventPayload, string>(builder);
    }
}
