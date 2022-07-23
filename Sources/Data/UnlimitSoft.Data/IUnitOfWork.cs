using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace UnlimitSoft.Data
{
    /// <summary>
    /// 
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        int SaveChanges();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<int> SaveChangesAsync(CancellationToken ct = default);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerable<Type> GetModelEntityTypes();
    }
}
