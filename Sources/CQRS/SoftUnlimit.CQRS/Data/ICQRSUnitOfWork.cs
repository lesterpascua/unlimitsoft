using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.EventSourcing;
using SoftUnlimit.Data;

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
