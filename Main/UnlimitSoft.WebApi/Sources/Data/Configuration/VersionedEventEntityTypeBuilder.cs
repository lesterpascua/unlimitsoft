using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnlimitSoft.Data.EntityFramework.Utility;
using UnlimitSoft.Message;

namespace UnlimitSoft.WebApi.Sources.Data.Configuration;


public class VersionedEventEntityTypeBuilder : _EntityTypeBuilder<EventPayload>
{
    public override void Configure(EntityTypeBuilder<EventPayload> builder) => EntityBuilderUtility.ConfigureEvent<EventPayload>(builder);
}
