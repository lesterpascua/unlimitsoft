using Microsoft.Extensions.Logging;
using UnlimitSoft.Cache;
using UnlimitSoft.Json;
using UnlimitSoft.Message;
using UnlimitSoft.Web.Client;

namespace OIU.Libs.Web;


/// <summary>
/// Base class for all api services in onejn system
/// </summary>
/// <example> 
/// <![CDATA[
/// public sealed class MyApi : BaseApiServiceX
/// {
///     public MyApi(IApiClient apiClient, ILogger<MyApi>? logger = null)
///         : base(apiClient, logger: logger)
///     {
///     }
///     
///     // for service with out cache
///     public Task<Result<TagModel[]>> GetAsync(CancellationToken ct = default)
///     {
///         var action = () => SendAsync<TagModel[]>(
///             HttpMethod.Get,
///             "api/v3/application",
///             null,
///             ct
///         );
///         return Http.GetResultAsync(ApiClient.Serializer, action, Logger);
///     }
///     
///     // for service with cache
///     public Task<Result<TagModel[]>> GetAsync(CancellationToken ct = default)
///     {
///         var action = BuildWithCheckCacheAsync<TagModel[]>(
///             HttpMethod.Get, 
///             "api/v3/application", 
///             null,
///             ct
///         );
///         return Http.GetResultAsync(ApiClient.Serializer, action, Logger);
///     }
/// }
/// public static class Programs
/// {
///     public static async Task Main()
///     {
///         var apiKey = "secret";
///         var identity = IdentityInfo.System;
///         var api = new MyApi(new DefaultApiClient(new HttpClient(), JsonUtil.Default), null)
///         {
///             OnBeforeRequest = (message, correlationId) => message.AddApiKeyHeader(JsonUtil.Default, identity, apiKey, correlationId)
///         };
/// 
///         await api.CreateAsync(1);
///     }
/// }
/// ]]>
/// </example>
public abstract class BaseApiServiceX : BaseApiService
{

    /// <summary>
    /// 
    /// </summary>
    /// <param name="apiClient"></param>
    /// <param name="cache"></param>
    /// <param name="ignorePrevCache"></param>
    /// <param name="middleware"></param>
    /// <param name="logger"></param>
    protected BaseApiServiceX(IApiClient apiClient, ICache? cache = null, bool ignorePrevCache = false, IHttpRequestMiddleware? middleware = null, ILogger<BaseApiServiceX>? logger = null)
        : base(apiClient, cache, ignorePrevCache, logger)
    {
        Middleware = middleware;
    }

    /// <summary>
    /// Callback to modify the request before send it
    /// </summary>
    public IHttpRequestMiddleware? Middleware { get; }

    #region Protected Methods
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    protected virtual void Setup(HttpRequestMessage message)
    {
        Middleware?.Update(message);
    }
    /// <summary>
    /// Get the expiration time for the cache acording with the supplied key
    /// </summary>
    protected virtual TimeSpan? GetAbsoluteExpirationRelativeToNow(string key)
    {
        return null;
    }
    /// <summary>
    /// Send http request from the cache. Method always is GET
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="url"></param>
    /// <param name="key"></param>
    /// <param name="correlationId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    protected async Task<Result<T>> SendFromCacheAsync<T>(string url, string? key, string? correlationId = null, CancellationToken ct = default)
    {
        key ??= typeof(T).FullName!;

        if (Cache is null)
            return await SendAsync<T>(HttpMethod.Get, url, correlationId: correlationId, ct: ct);

        var expirationTime = GetAbsoluteExpirationRelativeToNow(key);
        if (expirationTime is null)
            return await SendAsync<T>(HttpMethod.Get, url, correlationId: correlationId, ct: ct);

        return await TryCacheFirst(
            _ => SendAsync<T>(HttpMethod.Get, url, correlationId: correlationId, ct: ct),
            (ref ICache.Config config) =>
            {
                config.AbsoluteExpirationRelativeToNow = expirationTime;
                return true;
            },
            key
        );
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="method"></param>
    /// <param name="uri"></param>
    /// <param name="model"></param>
    /// <param name="serializer"></param>
    /// <param name="correlationId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    protected Task<Result<T>> SendAsync<T>(HttpMethod method, string uri, object? model = null, IJsonSerializer? serializer = null, string? correlationId = null, CancellationToken ct = default)
    {
        return Http.GetResultAsync(
            serializer ?? ApiClient.Serializer,
            () => ApiClient.SendAsync<T>(
                method, 
                uri, 
                request =>
                {
                    request.AddCorrelationId(correlationId);
                    Setup(request);
                }, 
                model, 
                serializer, 
                ct
            ).ContinueWith(c => c.Result.Item1!),
            Logger
        );
    }
    #endregion
}
