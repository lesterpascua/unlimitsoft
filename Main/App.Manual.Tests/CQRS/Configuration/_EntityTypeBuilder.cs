using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace App.Manual.Tests.CQRS.Configuration
{
#pragma warning disable IDE1006 // Naming Styles
    public abstract class _EntityTypeBuilder<TEntity> : IEntityTypeConfiguration<TEntity>
#pragma warning restore IDE1006 // Naming Styles
         where TEntity : class
    {
        public abstract void Configure(EntityTypeBuilder<TEntity> builder);
    }
}
