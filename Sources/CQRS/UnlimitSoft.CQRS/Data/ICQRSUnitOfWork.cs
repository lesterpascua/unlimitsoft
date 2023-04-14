using UnlimitSoft.CQRS.Event;
using UnlimitSoft.Data;

namespace UnlimitSoft.CQRS.Data;


/// <summary>
/// Unit of work with the CQRS patter integrate
/// </summary>
public interface ICQRSUnitOfWork : IUnitOfWork, IAdvancedUnitOfWork
{
    /// <summary>
    /// 
    /// </summary>
    IMediatorDispatchEvent? EventMediator { get; }
}
