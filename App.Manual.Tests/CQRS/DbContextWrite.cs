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
    public class DbContextWrite : DbContext, IDbContextHook
    {
        private Action<ModelBuilder> _builderAction;


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
        /// <param name="action"></param>
        public void OnModelCreatingCallback(Action<ModelBuilder> action)
        {
            _builderAction = action;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            _builderAction?.Invoke(builder);
        }
    }
}
