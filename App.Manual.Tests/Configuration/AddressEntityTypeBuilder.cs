using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoftUnlimit.CQRS.Test.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.CQRS.Test.Configuration
{
    public class AddressEntityTypeBuilder : _EntityTypeBuilder<Address>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        public override void Configure(EntityTypeBuilder<Address> builder)
        {
            base.Configure(builder);

            builder.HasKey(k => k.ID);

            builder.Property(p => p.ID).ValueGeneratedNever();
            builder.Property(p => p.Street).IsRequired().HasMaxLength(60);

            builder.HasOne(p => p.Customer).WithMany(p => p.Addresses).HasForeignKey(k => k.CustomerID).IsRequired().OnDelete(DeleteBehavior.Cascade);
        }
    }
}
