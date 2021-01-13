using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SoftUnlimit.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.Data.EntityFramework
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDbContextWrapper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        DbContext GetDbContext();
    }
    /// <summary>
    /// 
    /// </summary>
    public abstract class EFDbUnitOfWork<TDbContext> : IDbContextWrapper, IUnitOfWork
        where TDbContext : DbContext, IDbContextHook
    {
        #region Ctor

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbContext"></param>
        protected EFDbUnitOfWork(TDbContext dbContext)
        {
            DbContext = dbContext;
            dbContext.OnModelCreatingCallback(OnModelCreating);
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// Internal db context.
        /// </summary>
        protected TDbContext DbContext { get; }
        /// <summary>
        /// Base class use as parameter for find EntityTypeBuilder classes para conformar el modelo.
        /// </summary>
        protected abstract Type EntityTypeBuilderBaseClass { get; }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DbContext GetDbContext() => DbContext;

        #region Protected Methods

        /// <summary>
        /// Check type for extra contrains and return if is valid for this DbContext.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected abstract bool AcceptConfigurationType(Type type);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected virtual void OnModelCreating(ModelBuilder modelBuilder) => DbContext.OnModelCreating(EntityTypeBuilderBaseClass, modelBuilder, AcceptConfigurationType);

        #endregion

        /// <summary>
        /// Dispose internal db context.
        /// </summary>
        public virtual void Dispose() => DbContext.Dispose();
        /// <summary>
        /// Get all types inside of this unit of work.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<Type> GetModelEntityTypes() => DbContext.Model.GetEntityTypes().Select(s => s.ClrType);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual int SaveChanges() => DbContext.SaveChanges();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => DbContext.SaveChangesAsync(cancellationToken);

        
    }
}
