using SoftUnlimit.Web.Client;
using System.Net.Http;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.WebApi.Sources.Adapter
{
    public record MockResponse(int StatusCode, string Description);

    public interface IMockService : IApiService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vm"></param>
        /// <param name="canned"></param>
        /// <returns></returns>
        Task<MockResponse> Request200(CancellationToken ct = default);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vm"></param>
        /// <param name="canned"></param>
        /// <returns></returns>
        Task<MockResponse> Request202(CancellationToken ct = default);
    }
    public class MockService : BaseApiService, IMockService
    {
        public MockService(IApiClient apiClient, ObjectCache cache, CacheItemPolicy cacheItemPolicy, bool ignorePrevCache = false)
            : base(apiClient, cache, cacheItemPolicy, ignorePrevCache)
        {
        }

        public async Task<MockResponse> Request200(CancellationToken ct = default)
        {
            var (response, code) = await ApiClient.SendAsync<MockResponse>(HttpMethod.Get, "https://mock.codes/200", ct: ct);
            return response;
        }
        public async Task<MockResponse> Request202(CancellationToken ct = default)
        {
            var (response, code) = await ApiClient.SendAsync<MockResponse>(HttpMethod.Get, "https://mock.codes/202", ct: ct);
            return response;
        }
    }
}
