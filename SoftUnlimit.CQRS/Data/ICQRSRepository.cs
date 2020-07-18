using SoftUnlimit.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Data
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface ICQRSRepository<TEntity> : IRepository<TEntity>
        where TEntity : class, IAggregateRoot
    {
    }
}
