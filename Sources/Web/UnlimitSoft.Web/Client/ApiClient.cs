using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.Json;

namespace UnlimitSoft.Web.Client;


/// <summary>
/// 
/// </summary>
public interface IApiClient : IDisposable
{
    /// <summary>
    /// Get internal http client resource.
    /// </summary>
    HttpClient HttpClient { get; }
    /// <summary>
    /// Serializer user to serialize and deserialize the response and the request
    /// </summary>
    IJsonSerializer Serializer { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <param name="request"></param>
    /// <param name="serializer"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<(TModel?, HttpStatusCode)> SendAsync<TModel>(Func<CancellationToken, Task<HttpResponseMessage>> request, IJsonSerializer? serializer = null, CancellationToken ct = default);
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <param name="method"></param>
    /// <param name="uri"></param>
    /// <param name="setup">Allow configuration of current execution context before perform the request.</param>
    /// <param name="model"></param>
    /// <param name="serializer"></param>
    /// <param name="ct"></param>
    /// <exception cref="HttpException">If the error code is diferent of success.</exception>
    /// <returns></returns>
    Task<(TModel?, HttpStatusCode)> SendAsync<TModel>(HttpMethod method, string uri, Action<HttpRequestMessage>? setup = null, object? model = null, IJsonSerializer? serializer = null, CancellationToken ct = default);
    /// <summary>
    /// Upload file (multipart/form-data)
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <param name="method"></param>
    /// <param name="uri"></param>
    /// <param name="fileName"></param>
    /// <param name="streams"></param>
    /// <param name="setup">Allow configuration of current execution context before perform the request.</param>
    /// <param name="qs"></param>
    /// <param name="serializer"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<(TModel?, HttpStatusCode)> UploadAsync<TModel>(HttpMethod method, string uri, string fileName, IEnumerable<Stream> streams, Action<HttpRequestMessage>? setup = null, object? qs = null, IJsonSerializer? serializer = null, CancellationToken ct = default);
}
