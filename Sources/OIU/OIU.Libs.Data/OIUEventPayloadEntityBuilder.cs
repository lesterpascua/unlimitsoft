using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OIU.Libs.Events;
using UnlimitSoft.Data.EntityFramework.Utility;

namespace OIU.Libs.Data.SQLServer;


/// <summary>
/// 
/// </summary>
public static class OIUEventPayloadEntityBuilder
{
    /// <summary>
    /// Configure event to bind with entity framework
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="bodyLength"></param>
    public static void ConfigureEvent(EntityTypeBuilder<OIUEventPayload> builder, int bodyLength = int.MaxValue)
    {
        EntityBuilderUtility.ConfigureEvent(
            builder,
            true,
            false,
            builder =>
            {
                if (bodyLength != int.MaxValue)
                    builder.HasMaxLength(bodyLength);
            }
        );
        builder.Property(p => p.IdentityId).HasComment("Identifier of the entity owner of the event");
        builder.Property(p => p.SourceMicroServiceId).HasComment("Micro service where the event is originate in first place. Some event like scan file for virus move over several service this allow us to identify the original service where the event was created");
    }
}
