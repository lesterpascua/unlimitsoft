using App.Manual.Tests.CQRS.Configuration;
using Microsoft.EntityFrameworkCore;
using SoftUnlimit.CQRS.EventSourcing;
using SoftUnlimit.Data;
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
    public sealed class DbContextWrite : EFCQRSDbContext, IUnitOfWork
    {
        #region Ctor

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <param name="eventMediator"></param>
        /// <param name="eventSourcedMediator"></param>
        public DbContextWrite([NotNull] DbContextOptions<DbContextWrite> options, IMediatorDispatchEventSourced eventSourcedMediator)
            : base(options, null, eventSourcedMediator)
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

        #endregion
    }
}
