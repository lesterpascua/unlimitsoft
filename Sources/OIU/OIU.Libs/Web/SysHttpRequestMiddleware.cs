using Microsoft.Extensions.Options;
using OIU.Libs.Security;
using UnlimitSoft.Json;

namespace OIU.Libs.Web;


/// <summary>
/// 
/// </summary>
public sealed class SysHttpRequestMiddleware : IHttpRequestMiddleware
{
    private readonly AuthorizeOptions _options;
    private readonly IJsonSerializer _serializer;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="options"></param>
    /// <param name="serializer"></param>
    public SysHttpRequestMiddleware(IOptions<AuthorizeOptions> options, IJsonSerializer serializer)
    {
        _options = options.Value;
        _serializer = serializer;
    }

    /// <summary>
    /// Add the api key header to the request
    /// </summary>
    /// <param name="request"></param>
    public void Update(HttpRequestMessage request)
    {
        request.AddApiKeyHeader(_serializer, Identity.GetSystem(), _options.ApiKey);
    }
}
