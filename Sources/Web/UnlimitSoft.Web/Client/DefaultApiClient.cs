using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.Json;

namespace UnlimitSoft.Web.Client;


/// <summary>
/// 
/// </summary>
public class DefaultApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IJsonSerializer _serializer;
    private ILogger? _logger;


    /// <summary>
    /// 
    /// </summary>
    /// <param name="httpClient"></param>
    /// <param name="serializer"></param>
    /// <param name="baseUrl"></param>
    public DefaultApiClient(HttpClient httpClient, IJsonSerializer serializer, string? baseUrl = null)
    {
        _httpClient = httpClient;
        _serializer = serializer;
        if (!string.IsNullOrEmpty(baseUrl))
        {
            if (baseUrl![^1] != '/')
                baseUrl += '/';
            httpClient.BaseAddress = new Uri(baseUrl);
        }
    }

    /// <inheritdoc />
    public HttpClient HttpClient => _httpClient;
    /// <inheritdoc />
    public IJsonSerializer Serializer => _serializer;

    /// <inheritdoc />
    public void Dispose()
    {
        _httpClient.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public async Task<(TModel?, HttpStatusCode)> SendAsync<TModel>(Func<CancellationToken, Task<HttpResponseMessage>> request, IJsonSerializer? serializer = null, CancellationToken ct = default)
    {
        using var response = await request(ct);

        response.EnsureSuccessStatusCode();

#if NETSTANDARD2_0
        string body = await response.Content.ReadAsStringAsync();
#else
        string body = await response.Content.ReadAsStringAsync(ct);
#endif
        var jsonSerialize = serializer ?? _serializer;
        var result = jsonSerialize.Deserialize<TModel>(body);
        return (result, response.StatusCode);
    }
    /// <inheritdoc/>
    public async Task<(TModel?, HttpStatusCode)> SendAsync<TModel>(HttpMethod method, string uri, Action<HttpRequestMessage>? setup = null, object? model = null, IJsonSerializer? serializer = null, CancellationToken ct = default)
    {
        serializer ??= _serializer;
        var completeUri = GetUri(serializer, method, model, uri, out var jsonContent);

        HttpContent? httpContent = null;
        try
        {
            if (jsonContent is not null)
                httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var (json, code) = await SendWithContentAsync(method, completeUri, httpContent, setup, ct);
            var result = serializer.Deserialize<TModel>(json);

            _logger?.LogInformation("HttpResponse for {Url}, Result = {Code}, Body = {@Body}", completeUri, code, result);

            return (result, code);
        }
        finally
        {
            httpContent?.Dispose();
        }
    }

    /// <inheritdoc/>
    public async Task<(TModel?, HttpStatusCode)> UploadAsync<TModel>(HttpMethod method, string uri, string fileName, IEnumerable<Stream> streams, Action<HttpRequestMessage>? setup = null, object? qs = null, IJsonSerializer? serializer = null, CancellationToken ct = default)
    {
        if (method == HttpMethod.Get)
            throw new NotSupportedException("Get method don't allow upload image.");

        var completeUri = uri;
        var now = SysClock.GetUtcNow();
        var jsonSerialize = serializer ?? _serializer;
        var content = new MultipartFormDataContent($"Uploading... {now.ToString(CultureInfo.InvariantCulture)}");
        foreach (var stream in streams)
            content.Add(new StreamContent(stream), fileName, fileName);

        _logger?.LogInformation("HttpRequest for {Url}", completeUri);
        if (qs != null)
            completeUri += string.Concat('?', ObjectUtils.ToQueryString(_serializer, qs));

        var (json, code) = await SendWithContentAsync(method, completeUri, content, setup, ct);
        var result = jsonSerialize.Deserialize<TModel>(json);

        _logger?.LogInformation("HttpResponse for {Url}, Result = {Code}, Body = {@Body}", completeUri, code, result);

        return (result, code);
    }

    #region Internal Methods
    internal void SetLogger(ILogger logger) => _logger = logger;
    #endregion

    #region Private Methods
    private string GetUri(IJsonSerializer serializer, HttpMethod method, object? model, string uri, out string? content)
    {
        if (model is null)
        {
            content = null;
            _logger?.LogInformation("HttpRequest for {Url}", uri);
            return uri;
        }

        if (HttpMethod.Get == method)
        {
            content = null;
            var qs = ObjectUtils.ToQueryString(serializer, model);

            uri = $"{uri}?{qs}";
            _logger?.LogInformation("HttpRequest for {Url}", uri);
            return uri;
        }

        _logger?.LogInformation("HttpRequest for {Url}, Body = {@Body}", uri, model);
        content = serializer.Serialize(model);
        return uri;
    }
    private async Task<(string, HttpStatusCode)> SendWithContentAsync(HttpMethod method, string completeUri, HttpContent? content, Action<HttpRequestMessage>? setup, CancellationToken ct = default)
    {
        using var message = new HttpRequestMessage(method, completeUri);
        if (content is not null)
            message.Content = content;

        setup?.Invoke(message);
        using var response = await _httpClient.SendAsync(message, ct);

#if NETSTANDARD2_0
        string body = await response.Content.ReadAsStringAsync();
#else
        string body = await response.Content.ReadAsStringAsync(ct);
#endif

        if (!response.IsSuccessStatusCode)
        {
            _logger?.LogWarning("HttpResponse for {Url}, Result = {Code}, Body = {Body}", completeUri, response.StatusCode, body);
            throw new HttpException(response.StatusCode, response.ToString(), body);
        }

        return (body, response.StatusCode);
    }
    #endregion
}
