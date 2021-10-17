using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoftUnlimit.Cloud.Partner.Data.Model;
using SoftUnlimit.Data.EntityFramework.Utility;

namespace SoftUnlimit.Cloud.Partner.Data.Configuration
{
    public abstract class PartnerJobsEntityBuilder : _EntityTypeBuilder<PartnerJobs>
    {
        public override void Configure(EntityTypeBuilder<PartnerJobs> builder)
        {
            builder.ToTable(nameof(PartnerJobs));
            builder.HasKey(k => k.Id);

            builder.Property(p => p.JobId).HasComment("Hangfire job identifier");
            builder.Property(p => p.Created).HasComment("Date where the job was created").HasConversion(DateTimeUtcConverter.Instance);
        }
    }
}
