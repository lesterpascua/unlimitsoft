using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoftUnlimit.Cloud.VirusScan.Data.Model;

namespace SoftUnlimit.Cloud.VirusScan.Data.Configuration
{
    public class CompleteEntityTypeBuilder : _EntityTypeBuilder<Complete>
    {
        public override void Configure(EntityTypeBuilder<Complete> builder)
        {
            builder.ToTable(nameof(Complete));

            builder.HasKey(k => k.Id);

            builder.Property(p => p.CustomerId).HasComment("User owner of the file. Null if no user asociate");
            builder.Property(p => p.RequestId).HasComment("Identifier of the request");
            builder.Property(p => p.CorrelationId).HasMaxLength(40).HasComment("CorrelationId asociate to the process. Unique value to identifier the source of the operation");
            builder.Property(p => p.BlobUri).HasMaxLength(255).HasComment("Unique BlobUri identifier of the file");
            builder.Property(p => p.ScanStatus).HasComment("Indicate scanning status of the file");
            builder.Property(p => p.DownloadStatus).HasComment("Indicate download status of the file");
            builder.Property(p => p.Created).HasComment("Date where request is created");
            builder.Property(p => p.Scanned).HasComment("Date when the file was scanned");
            builder.Property(p => p.Retry).HasComment("Number of retry attemp for the file");

            builder.HasIndex(i => i.RequestId).IsUnique(false);
            builder.HasIndex(i => i.CorrelationId).IsUnique(false);

            builder.HasOne(p => p.Customer).WithMany().HasForeignKey(k => k.CustomerId).IsRequired(false).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
