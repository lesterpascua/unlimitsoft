using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoftUnlimit.Cloud.Partner.Data.Model;

namespace SoftUnlimit.Cloud.Partner.Data.Configuration
{
    public sealed class SaleforceCompleteEntityBuilder : _EntityTypeBuilder<SaleforceComplete>
    {
        public override void Configure(EntityTypeBuilder<SaleforceComplete> builder) => Utility.ConfigureComplete(builder, nameof(SaleforceComplete));
    }
}
