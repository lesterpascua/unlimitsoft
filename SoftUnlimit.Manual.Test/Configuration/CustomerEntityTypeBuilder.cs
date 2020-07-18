using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoftUnlimit.CQRS.Test.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.CQRS.Test.Configuration
{
    public class CustomerEntityTypeBuilder : _EntityTypeBuilder<Customer>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        public override void Configure(EntityTypeBuilder<Customer> builder)
        {
            base.Configure(builder);

            builder.HasKey(k => k.ID);

            builder.Property(p => p.ID).ValueGeneratedNever();
            builder.Property(p => p.FirstName).IsRequired().HasMaxLength(60);
            builder.Property(p => p.LastName).IsRequired().HasMaxLength(120);
            builder.Property(p => p.CID).IsRequired().HasMaxLength(32);
        }
    }
}
