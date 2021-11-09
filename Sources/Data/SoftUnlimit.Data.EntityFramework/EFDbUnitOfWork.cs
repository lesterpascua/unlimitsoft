using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
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
    public abstract class EFDbUnitOfWork<TDbContext> : IDbContextWrapper, IUnitOfWork, IDbConnectionFactory, IAdvancedUnitOfWork
        where TDbContext : DbContext
    {
        private int? _timeOut;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbContext"></param>
        protected EFDbUnitOfWork(TDbContext dbContext)
        {
            DbContext = dbContext;
        }


        /// <inheritdoc />
        public int TimeOut
        {
            get
            {
                if (!_timeOut.HasValue)
                    _timeOut = GetTimeOutFromConnectionString(DbContext.Database.GetConnectionString());
                return _timeOut.Value;
            }
        }
        /// <summary>
        /// Internal db context.
        /// </summary>
        protected TDbContext DbContext { get; private set; }

        /// <inheritdoc />
        public async Task TransactionCommitAsync(CancellationToken ct) => await DbContext.Database.CurrentTransaction.CommitAsync(ct);
        /// <inheritdoc />
        public async Task TransactionRollbackAsync(CancellationToken ct) => await DbContext.Database.CurrentTransaction.RollbackAsync(ct);
        /// <inheritdoc />
        public async Task<IDisposable> TransactionCreateAsync(IsolationLevel level, CancellationToken ct) => await DbContext.Database.BeginTransactionAsync(level, ct);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DbContext GetDbContext() => DbContext;
        /// <inheritdoc />
        public IDbConnection GetDbConnection() => DbContext.Database.GetDbConnection();
        /// <inheritdoc />
        public virtual IDbConnection CreateNewDbConnection() => throw new NotImplementedException("Implement this Create to enable this feature");

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && DbContext != null)
            {
                DbContext.Dispose();
                DbContext = null;
            }
        }

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
        public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => await DbContext.SaveChangesAsync(cancellationToken);

        /// <summary>
        /// Get connection string builder asociate to string.
        /// </summary>
        /// <param name="connString"></param>
        /// <returns>Return amount of second for execution command time out.</returns>
        protected virtual int GetTimeOutFromConnectionString(string connString) => throw new NotImplementedException("Implement GetTimeOutFromConnectionString to enable this feature.");
    }
}
