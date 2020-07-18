using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.Data
{
    /// <summary>
    /// Represent a rich UnitOfWork.
    /// </summary>
    public interface IAdvancedUnitOfWork
    {
        /// <summary>
        /// Commit hight level transaction
        /// </summary>
        /// <returns></returns>
        Task TransactionCommitAsync();
        /// <summary>
        /// Reject hight level transaction
        /// </summary>
        /// <returns></returns>
        Task TransactionRollbackAsync();
        /// <summary>
        /// Create a hight level transaction inside the transactional life cicle.
        /// </summary>
        /// <returns></returns>
        Task<IAsyncDisposable> TransactionCreateAsync();
    }
}
