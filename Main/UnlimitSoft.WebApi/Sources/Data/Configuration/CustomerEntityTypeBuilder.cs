using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnlimitSoft.WebApi.Sources.Data.Model;

namespace UnlimitSoft.WebApi.Sources.Data.Configuration
{
    public class CustomerEntityTypeBuilder : _EntityTypeBuilder<Customer>
    {
        public override void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.ToTable(nameof(Customer));
            builder.HasKey(k => k.Id);

            builder.Property(p => p.Name).HasMaxLength(64);
        }
    }
}
