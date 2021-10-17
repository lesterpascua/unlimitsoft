using SoftUnlimit.Web.Client;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.Cloud.Partner.Saleforce.Sender
{
    /// <summary>
    /// 
    /// </summary>
    public interface IAuthApiService : IApiService
    {
        /// <summary>
        /// Name of the event mapped in saleforce
        /// </summary>
        public string EventName { get; }
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public Task<string> GetTokenAsync(CancellationToken ct = default);
    }
}
