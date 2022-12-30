using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnlimitSoft.CQRS.Event.Json;
using UnlimitSoft.Data.EntityFramework.Utility;

namespace UnlimitSoft.WebApi.Sources.Data.Configuration;


public class VersionedEventEntityTypeBuilder : _EntityTypeBuilder<JsonEventPayload>
{
    public override void Configure(EntityTypeBuilder<JsonEventPayload> builder) => EntityBuilderUtility.ConfigureEvent<JsonEventPayload, string>(builder);
}
