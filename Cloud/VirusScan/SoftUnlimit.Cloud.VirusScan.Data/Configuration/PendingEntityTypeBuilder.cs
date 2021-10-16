using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoftUnlimit.Cloud.VirusScan.Data.Model;

namespace SoftUnlimit.Cloud.VirusScan.Data.Configuration
{
    public class PendingEntityTypeBuilder : _EntityTypeBuilder<Pending>
    {
        public override void Configure(EntityTypeBuilder<Pending> builder)
        {
            builder.ToTable(nameof(Pending));

            builder.HasKey(k => k.Id);

            builder.Property(p => p.CustomerId).HasComment("User owner of the file. Null if no user asociate");
            builder.Property(p => p.RequestId).HasComment("Identifier of the request");
            builder.Property(p => p.CorrelationId).HasMaxLength(40).HasComment("CorrelationId asociate to the process. Unique value to identifier the source of the operation");
            builder.Property(p => p.BlobUri).HasMaxLength(255).HasComment("Unique BlobUri identifier of the file");
            builder.Property(p => p.Status).HasComment("Status of the request. (1 - Pending, 2 - Approved, 3 - Error)");
            builder.Property(p => p.Created).HasComment("Date where request is created");
            builder.Property(p => p.Scheduler).HasComment("Date where the file will be scanned");
            builder.Property(p => p.Retry).HasComment("Number of retry attemp for the file");
            builder.Property(p => p.Metadata).HasComment("Metadata asociate to the file, serialize in json");

            builder.HasIndex(i => i.RequestId).IsUnique(false);
            builder.HasIndex(i => i.CorrelationId).IsUnique(false);

            builder.HasOne(p => p.Customer).WithMany().HasForeignKey(k => k.CustomerId).IsRequired(false).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
