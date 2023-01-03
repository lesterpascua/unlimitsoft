using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnlimitSoft.WebApi.MultiTenant.Sources.Data.Model;

namespace UnlimitSoft.WebApi.MultiTenant.Sources.Data.Configuration;


public sealed class CustomerEntityTypeBuilder : _EntityTypeBuilder<Customer>
{
    public override void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(k => k.Id);

        builder.Property(p => p.TenantId).HasComment("Indicate the tenant asociate to some customer");
        builder.Property(p => p.FirstName).IsRequired().HasMaxLength(40).HasComment("Customer first name");
        builder.Property(p => p.LastName).IsRequired().HasMaxLength(80).HasComment("Customer last name");

        builder.HasIndex(i => i.TenantId).IsUnique(false);
    }
}
