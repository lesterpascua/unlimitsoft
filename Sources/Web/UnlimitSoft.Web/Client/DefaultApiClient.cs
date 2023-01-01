using Microsoft.Extensions.Logging;
using UnlimitSoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UnlimitSoft.Web.Client;


/// <summary>
/// 
/// </summary>
public class DefaultApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IJsonSerializer _serializer;
    private readonly ILogger<DefaultApiClient>? _logger;


    /// <summary>
    /// 
    /// </summary>
    /// <param name="httpClient"></param>
    /// <param name="serializer"></param>
    /// <param name="baseUrl"></param>
    /// <param name="logger"></param>
    public DefaultApiClient(HttpClient httpClient, IJsonSerializer serializer, string? baseUrl = null, ILogger<DefaultApiClient>? logger = null)
    {
        _httpClient = httpClient;
        _serializer = serializer;
        _logger = logger;
        if (!string.IsNullOrEmpty(baseUrl))
        {
            if (baseUrl![baseUrl.Length - 1] != '/')
                baseUrl += '/';
            httpClient.BaseAddress = new Uri(baseUrl);
        }
    }

    /// <summary>
    /// Expose internal HttpClient
    /// </summary>
    public HttpClient HttpClient => _httpClient;

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
        string completeUri = uri;
        string? jsonContent = null;
        var jsonSerialize = serializer ?? _serializer;

        if (model is not null)
        {
            if (HttpMethod.Get == method)
            {
                var qs = await ObjectUtils.ToQueryString(_serializer, model);
                completeUri = $"{completeUri}?{qs}";
            }
            else
                jsonContent = jsonSerialize.Serialize(model);
        }
        HttpContent? httpContent = null;
        try
        {
            _logger?.LogInformation("HttpRequest for {Url}, Body = {Json}", completeUri, jsonContent);
            if (jsonContent is not null)
                httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var (json, code) = await SendWithContentAsync(method, completeUri, httpContent, setup, ct);

            _logger?.LogInformation("HttpResponse for {Url}, Result = {Code}, Body = {Json}", completeUri, code, json);
            var result = jsonSerialize.Deserialize<TModel>(json);

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

        if (qs != null)
            completeUri += string.Concat('?', await ObjectUtils.ToQueryString(_serializer, qs));

        var (json, code) = await SendWithContentAsync(method, completeUri, content, setup, ct);
        var result = jsonSerialize.Deserialize<TModel>(json);

        return (result, code);
    }



    #region Private Methods
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
            throw new HttpException(response.StatusCode, response.ToString(), body);

        return (body, response.StatusCode);
    }
#endregion
}
