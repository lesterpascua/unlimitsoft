﻿using Microsoft.EntityFrameworkCore;
using SoftUnlimit.Cloud.Partner.Data.Configuration;
using SoftUnlimit.Data.EntityFramework;
using System.Diagnostics.CodeAnalysis;

namespace SoftUnlimit.Cloud.Partner.Data
{
    public sealed class DbContextWrite : DbContext
    {
        public DbContextWrite([NotNull] DbContextOptions options) : base(options)
        {
        }

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
