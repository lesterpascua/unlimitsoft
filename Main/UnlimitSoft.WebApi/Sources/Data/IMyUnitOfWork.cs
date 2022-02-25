using SoftUnlimit.CQRS.Data;
using SoftUnlimit.Data;

namespace SoftUnlimit.WebApi.Sources.Data
{
    public interface IMyUnitOfWork : ICQRSUnitOfWork, IDbConnectionFactory
    {
    }
}
