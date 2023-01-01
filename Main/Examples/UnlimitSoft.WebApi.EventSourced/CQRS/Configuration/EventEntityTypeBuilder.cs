using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnlimitSoft.CQRS.Event.Json;
using UnlimitSoft.Data.EntityFramework.Utility;

namespace UnlimitSoft.WebApi.EventSourced.CQRS.Configuration;


public sealed class EventEntityTypeBuilder : _EntityTypeBuilder<JsonEventPayload>
{
    public override void Configure(EntityTypeBuilder<JsonEventPayload> builder)
    {
        EntityBuilderUtility.ConfigureEvent<JsonEventPayload, string>(builder);

        builder.HasIndex(p => p.SourceId);
        builder.HasIndex(p => new { p.SourceId, p.Version });
        builder.HasIndex(p => new { p.SourceId, p.Created });
    }
}
