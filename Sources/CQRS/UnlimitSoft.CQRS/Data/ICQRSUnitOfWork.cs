using UnlimitSoft.CQRS.Event;
using UnlimitSoft.Data;

namespace UnlimitSoft.CQRS.Data;


/// <summary>
/// 
/// </summary>
public interface ICQRSUnitOfWork : IUnitOfWork, IAdvancedUnitOfWork
{
    /// <summary>
    /// 
    /// </summary>
    IMediatorDispatchEvent? EventMediator { get; }
}
