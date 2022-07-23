using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnlimitSoft.CQRS.EventSourcing.Json;
using UnlimitSoft.Data.EntityFramework.Utility;

namespace UnlimitSoft.WebApi.Sources.Data.Configuration
{
    public class VersionedEventEntityTypeBuilder : _EntityTypeBuilder<JsonVersionedEventPayload>
    {
        public override void Configure(EntityTypeBuilder<JsonVersionedEventPayload> builder) 
            => EntityBuilderUtility.ConfigureVersionedEvent<JsonVersionedEventPayload, string>(builder);
    }
}
