using UnlimitSoft.Data;

namespace UnlimitSoft.WebApi.EventSourced.CQRS.Data;


public interface IMyUnitOfWork : IUnitOfWork, IDbConnectionFactory
{
}
