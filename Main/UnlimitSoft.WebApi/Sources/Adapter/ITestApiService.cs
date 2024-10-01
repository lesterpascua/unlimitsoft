using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.Logger;
using UnlimitSoft.Web.Client;

namespace UnlimitSoft.WebApi.Sources.Adapter;


public record TestApiResponse
{
    public int StatusCode { get; set; }
    [IsSensitive]
    public string? Description { get; set; }
}

public interface ITestApiService : IApiService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="vm"></param>
    /// <param name="canned"></param>
    /// <returns></returns>
    Task<TestApiResponse> Request200(CancellationToken ct = default);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="vm"></param>
    /// <param name="canned"></param>
    /// <returns></returns>
    Task<TestApiResponse> Request202(CancellationToken ct = default);
}
public class TestService : BaseApiService, ITestApiService
{
    public TestService(IApiClient apiClient, ILogger<TestService> logger)
        : base(apiClient, logger: logger)
    {
    }

    public async Task<TestApiResponse> Request200(CancellationToken ct = default)
    {
        var (response, code) = await ApiClient.SendAsync<TestApiResponse>(HttpMethod.Get, "https://mock.codes/200", ct: ct);
        return response!;
    }
    public async Task<TestApiResponse> Request202(CancellationToken ct = default)
    {
        var (response, code) = await ApiClient.SendAsync<TestApiResponse>(HttpMethod.Get, "https://mock.codes/202", ct: ct);
        return response!;
    }
}
