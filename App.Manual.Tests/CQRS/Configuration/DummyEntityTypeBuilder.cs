using App.Manual.Tests.CQRS.Data;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace App.Manual.Tests.CQRS.Configuration
{
    public class DummyEntityTypeBuilder : _EntityTypeBuilder<Dummy>
    {
        public override void Configure(EntityTypeBuilder<Dummy> builder)
        {
            builder.HasKey(k => k.ID);

            builder.Property(p => p.ID).ValueGeneratedNever();
            builder.Property(p => p.Name).IsRequired().HasMaxLength(60);
        }
    }
}
