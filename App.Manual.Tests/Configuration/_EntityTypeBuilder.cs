using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.CQRS.Test.Configuration
{
#pragma warning disable IDE1006 // Naming Styles
    /// <summary>
    /// Base class for type mapping
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public abstract class _EntityTypeBuilder<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : class
#pragma warning restore IDE1006 // Naming Styles
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        public virtual void Configure(EntityTypeBuilder<TEntity> builder)
        {
            builder.ToTable(typeof(TEntity).Name);
        }
    }
}
