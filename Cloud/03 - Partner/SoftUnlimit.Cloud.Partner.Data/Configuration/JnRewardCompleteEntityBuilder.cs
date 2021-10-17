using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoftUnlimit.Cloud.Partner.Data.Model;

namespace SoftUnlimit.Cloud.Partner.Data.Configuration
{
    public sealed class JnRewardCompleteEntityBuilder : _EntityTypeBuilder<JnRewardComplete>
    {
        public override void Configure(EntityTypeBuilder<JnRewardComplete> builder) => Utility.ConfigureComplete(builder, nameof(JnRewardComplete));
    }
}
