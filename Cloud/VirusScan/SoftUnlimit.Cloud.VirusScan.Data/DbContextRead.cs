using Microsoft.EntityFrameworkCore;
using SoftUnlimit.Cloud.VirusScan.Data.Configuration;
using SoftUnlimit.Data.EntityFramework;
using System.Diagnostics.CodeAnalysis;

namespace SoftUnlimit.Cloud.VirusScan.Data
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
