using UnlimitSoft.Data;

namespace UnlimitSoft.CQRS.Data;


/// <summary>
/// 
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface ICQRSRepository<TEntity> : IRepository<TEntity>
    where TEntity : class, IAggregateRoot
{
}
