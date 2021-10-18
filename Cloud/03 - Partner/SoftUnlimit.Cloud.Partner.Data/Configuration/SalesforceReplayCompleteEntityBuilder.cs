using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoftUnlimit.Cloud.Partner.Data.Model;

namespace SoftUnlimit.Cloud.Partner.Data.Configuration
{
    public sealed class SalesforceReplayCompleteEntityBuilder : _EntityTypeBuilder<SalesforceReplay>
    {
        public override void Configure(EntityTypeBuilder<SalesforceReplay> builder)
        {
            builder.ToTable(nameof(SalesforceReplay));

            builder.HasKey(k => k.Id);

            builder.Property(p => p.EventName).HasMaxLength(64).HasComment("Event name in salesforce (is the channel name)");
            builder.Property(p => p.ReplayId).HasComment("Max ReplayId receive using this EventName (is the channel name)");

            builder.HasIndex(i => i.EventName).IsUnique(true);
        }
    }
}
