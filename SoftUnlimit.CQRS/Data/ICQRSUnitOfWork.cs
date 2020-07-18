using SoftUnlimit.CQRS.Data;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.EventSourcing;
using SoftUnlimit.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Data
{
    /// <summary>
    /// 
    /// </summary>
    public interface ICQRSUnitOfWork : IUnitOfWork, IAdvancedUnitOfWork
    {
        /// <summary>
        /// 
        /// </summary>
        IMediatorDispatchEvent EventMediator { get; }
        /// <summary>
        /// 
        /// </summary>
        IMediatorDispatchEventSourced EventSourcedMediator { get; }
    }
}
