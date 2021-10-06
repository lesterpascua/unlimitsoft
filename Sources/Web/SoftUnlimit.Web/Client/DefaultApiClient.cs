using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.Web.Client
{
    /// <summary>
    /// 
    /// </summary>
    public class DefaultApiClient : IApiClient
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="baseUrl"></param>
        public DefaultApiClient(HttpClient httpClient, string baseUrl = null)
        {
            _httpClient = httpClient;
            if (!string.IsNullOrEmpty(baseUrl))
            {
                if (baseUrl[baseUrl.Length - 1] != '/')
                    baseUrl += '/';
                httpClient.BaseAddress = new Uri(baseUrl);
            }
        }

        /// <summary>
        /// Expose internal HttpClient
        /// </summary>
        public HttpClient HttpClient => _httpClient;

        /// <inheritdoc />
        public void Dispose() => HttpClient.Dispose();
        /// <inheritdoc/>
        public async Task<(TModel, HttpStatusCode)> SendAsync<TModel>(HttpMethod method, string uri, Action<HttpRequestMessage> setup = null, object model = null, CancellationToken ct = default)
        {
            string completeUri = uri;
            string jsonContent = null;

            if (model != null)
            {
                if (HttpMethod.Get == method)
                {
                    string qs = await ObjectUtils.ToQueryString(model);
                    completeUri = $"{completeUri}?{qs}";
                } else
                    jsonContent = JsonConvert.SerializeObject(model);
            }
            HttpContent httpContent = null;
            try
            {
                if (jsonContent != null)
                    httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                await BeforeSendRequestAsync(httpContent, method, completeUri, ct);
                return await SendWithContentAsync<TModel>(_httpClient, method, completeUri, httpContent, setup);
            }
            finally
            {
                if (httpContent != null)
                    httpContent.Dispose();
            }
        }
        /// <inheritdoc/>
        public async Task<(TModel, HttpStatusCode)> UploadAsync<TModel>(HttpMethod method, string uri, string fileName, IEnumerable<Stream> streams, Action<HttpRequestMessage> setup = null, object qs = null, CancellationToken ct = default)
        {
            if (method == HttpMethod.Get)
                throw new NotSupportedException("Get method don't allow upload image.");

            string completeUri = uri;
            var content = new MultipartFormDataContent("Uploading... " + DateTime.Now.ToString(CultureInfo.InvariantCulture));
            foreach (var stream in streams)
                content.Add(new StreamContent(stream), fileName, fileName);

            if (qs != null)
                completeUri += string.Concat('?', await ObjectUtils.ToQueryString(qs));

            await BeforeSendRequestAsync(content, method, uri);
            return await SendWithContentAsync<TModel>(_httpClient, method, completeUri, content, setup);
        }

        /// <summary>
        /// Performs operations before request.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="method"></param>
        /// <param name="uri"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        protected virtual Task BeforeSendRequestAsync(HttpContent content, HttpMethod method, string uri, CancellationToken ct = default) => Task.CompletedTask;

        #region Private Methods

        private static async Task<(TModel, HttpStatusCode)> SendWithContentAsync<TModel>(HttpClient httpClient, HttpMethod method, string completeUri, HttpContent content, Action<HttpRequestMessage> setup, CancellationToken ct = default)
        {
            using var message = new HttpRequestMessage(method, completeUri);
            if (content != null)
                message.Content = content;

            setup?.Invoke(message);
            using var response = await httpClient.SendAsync(message, ct);

            if (!response.IsSuccessStatusCode)
            {
                string body = await response.Content.ReadAsStringAsync();
                throw new HttpException(response.StatusCode, response.ToString(), body);
            }

            var json = await response.Content.ReadAsStringAsync();
            return (JsonConvert.DeserializeObject<TModel>(json), response.StatusCode);
        }

        #endregion
    }
}
