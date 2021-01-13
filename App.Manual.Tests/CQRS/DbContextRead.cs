using App.Manual.Tests.CQRS.Configuration;
using Microsoft.EntityFrameworkCore;
using SoftUnlimit.Data.EntityFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace App.Manual.Tests.CQRS
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
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            this.OnModelCreating(typeof(_EntityTypeBuilder<>), modelBuilder, _ => true);
        }
    }
}
