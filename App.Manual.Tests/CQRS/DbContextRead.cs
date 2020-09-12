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
    public sealed class DbContextRead : EFDbContext
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        public DbContextRead([NotNull] DbContextOptions<DbContextRead> options)
            : base(options)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        protected override Type EntityTypeBuilderBaseClass => typeof(_EntityTypeBuilder<>);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected override bool AcceptConfigurationType(Type type) => true;
    }
}
