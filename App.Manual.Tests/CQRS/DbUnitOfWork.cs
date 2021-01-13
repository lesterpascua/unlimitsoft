using App.Manual.Tests.CQRS.Configuration;
using Microsoft.EntityFrameworkCore;
using SoftUnlimit.CQRS.Data;
using SoftUnlimit.CQRS.EventSourcing;
using SoftUnlimit.Data;
using SoftUnlimit.Data.EntityFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace App.Manual.Tests.CQRS
{
    public interface IDbUnitOfWork : ICQRSUnitOfWork
    {
    }
    /// <summary>
    /// 
    /// </summary>
    public sealed class DbUnitOfWork : EFCQRSDbUnitOfWork<DbContextWrite>, IDbUnitOfWork
    {
        #region Ctor

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="eventSourcedMediator"></param>
        public DbUnitOfWork(DbContextWrite dbContext, IMediatorDispatchEventSourced eventSourcedMediator)
            : base(dbContext, null, eventSourcedMediator)
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
