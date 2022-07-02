using Microsoft.Extensions.Logging;
using SoftUnlimit.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.Web.Client;


/// <summary>
/// 
/// </summary>
public class DefaultApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DefaultApiClient>? _logger;


    /// <summary>
    /// 
    /// </summary>
    /// <param name="httpClient"></param>
    /// <param name="baseUrl"></param>
    /// <param name="logger"></param>
    public DefaultApiClient(HttpClient httpClient, string? baseUrl = null, ILogger<DefaultApiClient>? logger = null)
    {
        _httpClient = httpClient;
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
    public async Task<(TModel?, HttpStatusCode)> SendAsync<TModel>(Func<CancellationToken, Task<HttpResponseMessage>> request, CancellationToken ct = default)
    {
        using var response = await request(ct);

        response.EnsureSuccessStatusCode();
        string body = await response.Content.ReadAsStringAsync();

        var result = JsonUtility.Deserialize<TModel>(body);
        return (result, response.StatusCode);
    }

    /// <inheritdoc/>
    public Task<(TModel?, HttpStatusCode)> SendAsync<TModel>(HttpMethod method, string uri, Action<HttpRequestMessage>? setup = null, object? model = null, CancellationToken ct = default)
    {
        return InternalSendAsync(
            method,
            uri,
            obj => JsonUtility.Serialize(obj),
            json => JsonUtility.Deserialize<TModel>(json),
            setup,
            model,
            ct
        );
    }
    /// <inheritdoc />
    public Task<(TModel?, HttpStatusCode)> SendAsync<TModel>(HttpMethod method, string uri, System.Text.Json.JsonSerializerOptions serializer, System.Text.Json.JsonSerializerOptions deserializer, Action<HttpRequestMessage>? setup = null, object? model = null, CancellationToken ct = default)
    {
        if (serializer is null)
            throw new ArgumentNullException(nameof(serializer));
        if (deserializer is null)
            throw new ArgumentNullException(nameof(deserializer));

        return InternalSendAsync(
            method,
            uri,
            obj => System.Text.Json.JsonSerializer.Serialize(obj, serializer),
            json => System.Text.Json.JsonSerializer.Deserialize<TModel>(json, deserializer),
            setup,
            model,
            ct
        );
    }
    /// <inheritdoc />
    public Task<(TModel?, HttpStatusCode)> SendAsync<TModel>(HttpMethod method, string uri, Newtonsoft.Json.JsonSerializerSettings serializer, Newtonsoft.Json.JsonSerializerSettings deserializer, Action<HttpRequestMessage>? setup = null, object? model = null, CancellationToken ct = default)
    {
        if (serializer is null)
            throw new ArgumentNullException(nameof(serializer));
        if (deserializer is null)
            throw new ArgumentNullException(nameof(deserializer));

        return InternalSendAsync(
            method, 
            uri,
            obj => Newtonsoft.Json.JsonConvert.SerializeObject(obj, serializer),
            json => Newtonsoft.Json.JsonConvert.DeserializeObject<TModel>(json, deserializer), 
            setup, 
            model,
            ct
        );
    }

    /// <inheritdoc/>
    public async Task<(TModel?, HttpStatusCode)> UploadAsync<TModel>(HttpMethod method, string uri, string fileName, IEnumerable<Stream> streams, Action<HttpRequestMessage>? setup = null, object? qs = null, CancellationToken ct = default)
    {
        if (method == HttpMethod.Get)
            throw new NotSupportedException("Get method don't allow upload image.");

        var completeUri = uri;
        var content = new MultipartFormDataContent("Uploading... " + DateTime.Now.ToString(CultureInfo.InvariantCulture));
        foreach (var stream in streams)
            content.Add(new StreamContent(stream), fileName, fileName);

        if (qs != null)
            completeUri += string.Concat('?', await ObjectUtils.ToQueryString(qs));

        var (json, code) = await SendWithContentAsync(method, completeUri, content, setup, ct);
        var result = JsonUtility.Deserialize<TModel>(json);

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

        string body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new HttpException(response.StatusCode, response.ToString(), body);

        return (body, response.StatusCode);
    }
    private async Task<(TModel?, HttpStatusCode)> InternalSendAsync<TModel>(HttpMethod method, string uri, Func<object, string?> serializer, Func<string, TModel?> deserializer, Action<HttpRequestMessage>? setup, object? model, CancellationToken ct)
    {
        string completeUri = uri;
        string? jsonContent = null;

        if (model is not null)
        {
            if (HttpMethod.Get == method)
            {
                var qs = await ObjectUtils.ToQueryString(model);
                completeUri = $"{completeUri}?{qs}";
            }
            else
                jsonContent = serializer(model);
        }
        HttpContent? httpContent = null;
        try
        {
            _logger?.LogInformation("HttpRequest for {Url}, Body = {Json}", completeUri, jsonContent);
            if (jsonContent is not null)
                httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var (json, code) = await SendWithContentAsync(method, completeUri, httpContent, setup, ct);

            _logger?.LogInformation("HttpResponse for {Url}, Result = {Code}, Body = {Json}", completeUri, code, json);
            var result = deserializer(json);

            return (result, code);
        }
        finally
        {
            if (httpContent is not null)
                httpContent.Dispose();
        }
    }
    #endregion
}
