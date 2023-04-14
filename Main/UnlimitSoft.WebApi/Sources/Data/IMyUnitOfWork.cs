using UnlimitSoft.CQRS.Data;
using UnlimitSoft.Data;

namespace UnlimitSoft.WebApi.Sources.Data;


public interface IMyUnitOfWork : ICQRSUnitOfWork, IDbConnectionFactory
{
}
