using SoftUnlimit.Cloud.Partner.Saleforce.Sender.Model;
using SoftUnlimit.Web.Client;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.Cloud.Partner.Saleforce.Sender
{
    /// <summary>
    /// 
    /// </summary>
    public interface IEventPublisherApiService : IApiService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<(PublishStatus, HttpStatusCode)> PublishAsync(EventSignature e, CancellationToken ct = default);
    }
}
