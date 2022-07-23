using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace UnlimitSoft.Data
{
    /// <summary>
    /// Represent a rich UnitOfWork.
    /// </summary>
    public interface IAdvancedUnitOfWork
    {
        /// <summary>
        /// Commit hight level transaction
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task TransactionCommitAsync(CancellationToken ct = default);
        /// <summary>
        /// Reject hight level transaction
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task TransactionRollbackAsync(CancellationToken ct = default);
        /// <summary>
        /// Create a hight level transaction inside the transactional life cicle.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<IDisposable> TransactionCreateAsync(IsolationLevel level = IsolationLevel.ReadCommitted, CancellationToken ct = default);
    }
}
