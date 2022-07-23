using UnlimitSoft.CQRS.Event;
using UnlimitSoft.CQRS.EventSourcing;
using UnlimitSoft.Data;

namespace UnlimitSoft.CQRS.Data
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
