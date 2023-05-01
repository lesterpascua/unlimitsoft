using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnlimitSoft.Data.EntityFramework.Utility;
using UnlimitSoft.WebApi.EventSourced.CQRS.Repository;

namespace UnlimitSoft.WebApi.EventSourced.CQRS.Configuration;


public sealed class EventEntityTypeBuilder : _EntityTypeBuilder<MyEventPayload>
{
    public override void Configure(EntityTypeBuilder<MyEventPayload> builder)
    {
        EntityBuilderUtility.ConfigureEvent(builder, useExtraIndex: true);

        builder.Property(p => p.Text).IsRequired().HasMaxLength(30);
    }
}
