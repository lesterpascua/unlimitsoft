using SoftUnlimit.Cloud.Partner.Data.Model;
using SoftUnlimit.Cloud.Partner.Domain.Handler.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.Cloud.Partner.Domain.Handler.Services
{
    public interface IRoutingEvent
    {
        /// <summary>
        /// Type of the notification handling for the routing.
        /// </summary>
        NotificationType Type { get; }
        /// <summary>
        /// Send pending notification.
        /// </summary>
        /// <param name="partnerId"></param>
        /// <param name="pending"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<bool> RouteAsync(PartnerValues partnerId, Pending pending, CancellationToken ct = default);
    }
}
