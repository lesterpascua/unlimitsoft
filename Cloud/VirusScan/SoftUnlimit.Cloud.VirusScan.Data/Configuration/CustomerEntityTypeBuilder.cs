using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoftUnlimit.Cloud.VirusScan.Data.Model;

namespace SoftUnlimit.Cloud.VirusScan.Data.Configuration
{
    public class CustomerEntityTypeBuilder : _EntityTypeBuilder<Customer>
    {
        public override void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.ToTable(nameof(Customer));

            builder.HasKey(k => k.Id);

            builder.Property(p => p.VirusDetected).HasComment("Amount of request with virus detected from the FirstVirusDetected date");
            builder.Property(p => p.FirstVirusDetected).HasComment("Date when some request has mark with virus for first time");
            builder.Property(p => p.HistoryVirusDetected).HasComment("Amount of virus detected for this user in the entirely history");
        }
    }
}
