using SoftUnlimit.Cloud.Partner.Saleforce.Sender;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.Cloud.Partner.Domain.Handler.Services.Saleforce
{
    /// <summary>
    /// Build IEventPublisherApiService service asociate with partner.
    /// </summary>
    public interface IEventPublisherApiServiceFactory
    {
        /// <summary>
        /// Create or get the service match with the partner and the parameters setter to this partner.
        /// </summary>
        /// <param name="partnerId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<IEventPublisherApiService> CreateOrGetAsync(PartnerValues partnerId, CancellationToken ct = default);
    }
}
