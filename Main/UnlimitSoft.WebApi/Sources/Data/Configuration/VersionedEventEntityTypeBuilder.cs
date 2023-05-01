using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnlimitSoft.CQRS.Data.Dto;
using UnlimitSoft.Data.EntityFramework.Utility;

namespace UnlimitSoft.WebApi.Sources.Data.Configuration;


public class VersionedEventEntityTypeBuilder : _EntityTypeBuilder<EventPayload>
{
    public override void Configure(EntityTypeBuilder<EventPayload> builder) => EntityBuilderUtility.ConfigureEvent<EventPayload>(builder);
}
