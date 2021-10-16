using SoftUnlimit.CQRS.Data;
using SoftUnlimit.Data;

namespace SoftUnlimit.Cloud.VirusScan.Data
{
    public interface ICloudUnitOfWork : ICQRSUnitOfWork, IDbConnectionFactory
    {
    }
}
