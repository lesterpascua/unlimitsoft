using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoftUnlimit.WebApi.Sources.Data.Model;

namespace SoftUnlimit.WebApi.Sources.Data.Configuration
{
    public class LockEntityTypeBuilder : _EntityTypeBuilder<Lock>
    {
        public override void Configure(EntityTypeBuilder<Lock> builder)
        {
            builder.ToTable(nameof(Lock));
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id).ValueGeneratedNever();
            builder.Property(p => p.DateTime);
        }
    }
}
