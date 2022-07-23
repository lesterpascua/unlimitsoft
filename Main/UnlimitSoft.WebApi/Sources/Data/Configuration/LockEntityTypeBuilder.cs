using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnlimitSoft.WebApi.Sources.Data.Model;

namespace UnlimitSoft.WebApi.Sources.Data.Configuration
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
