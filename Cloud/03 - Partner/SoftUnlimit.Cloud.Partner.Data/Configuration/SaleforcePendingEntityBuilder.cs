using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoftUnlimit.Cloud.Partner.Data.Model;

namespace SoftUnlimit.Cloud.Partner.Data.Configuration
{
    public sealed class SaleforcePendingEntityBuilder : _EntityTypeBuilder<SaleforcePending>
    {
        public override void Configure(EntityTypeBuilder<SaleforcePending> builder) => Utility.ConfigurePending(builder, nameof(SaleforcePending));
    }
}
