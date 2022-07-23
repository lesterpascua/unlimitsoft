using Microsoft.EntityFrameworkCore;
using UnlimitSoft.Data.EntityFramework;
using UnlimitSoft.WebApi.Sources.Data.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace UnlimitSoft.WebApi.Sources.Data
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class DbContextRead : DbContext
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        public DbContextRead([NotNull] DbContextOptions<DbContextRead> options)
            : base(options)
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.HasDefaultSchema(DesignTimeDbContextFactory.Scheme);
            this.OnModelCreating(typeof(_EntityTypeBuilder<>), builder, _ => true);
        }
    }
}
