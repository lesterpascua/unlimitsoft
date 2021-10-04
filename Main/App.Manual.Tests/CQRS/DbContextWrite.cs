using App.Manual.Tests.CQRS.Configuration;
using Microsoft.EntityFrameworkCore;
using SoftUnlimit.Data.EntityFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace App.Manual.Tests.CQRS
{
    /// <summary>
    /// 
    /// </summary>
    public class DbContextWrite : DbContext
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        public DbContextWrite([NotNull] DbContextOptions<DbContextWrite> options)
            : base(options)
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            this.OnModelCreating(typeof(_EntityTypeBuilder<>), modelBuilder, _ => true);
        }
    }
}
