using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoftUnlimit.Cloud.Partner.Data.Model;

namespace SoftUnlimit.Cloud.Partner.Data.Configuration
{
    public sealed class JnRewardPendingEntityBuilder : _EntityTypeBuilder<JnRewardPending>
    {
        public override void Configure(EntityTypeBuilder<JnRewardPending> builder) => Utility.ConfigurePending(builder, nameof(JnRewardPending));
    }
}
