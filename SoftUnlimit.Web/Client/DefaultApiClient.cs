using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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
            this._httpClient = httpClient;
            if (!string.IsNullOrEmpty(baseUrl))
                httpClient.BaseAddress = new Uri(baseUrl);
        }

        /// <inheritdoc/>
        public async Task<TModel> SendAsync<TModel>(HttpMethod method, string uri, Action<HttpContent> setup = null, object model = null)
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
            HttpContent content = null;
            if (jsonContent != null)
                content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            await this.BeforeSendRequestAsync(content, method, uri);
            return await SendWithContentAsync<TModel>(_httpClient, method, completeUri, content, setup);
        }
        /// <inheritdoc/>
        public async Task<TModel> UploadAsync<TModel>(HttpMethod method, string uri, string fileName, IEnumerable<Stream> streams, Action<HttpContent> setup = null, object qs = null)
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
        /// <returns></returns>
        protected virtual Task BeforeSendRequestAsync(HttpContent content, HttpMethod method, string uri) => Task.CompletedTask;

        #region Private Methods

        private static async Task<TModel> SendWithContentAsync<TModel>(HttpClient httpClient, HttpMethod method, string completeUri, HttpContent content, Action<HttpContent> setup)
        {
            setup?.Invoke(content);

            HttpRequestMessage message = new HttpRequestMessage(method, completeUri);
            if (content != null)
                message.Content = content;
            var response = await httpClient.SendAsync(message);

            if (!response.IsSuccessStatusCode)
            {
                string body = await response.Content.ReadAsStringAsync();
                switch (response.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                    case HttpStatusCode.BadRequest:
                        throw new HttpException(response.StatusCode, response.ToString(), body);
                }
                throw new HttpException(HttpStatusCode.InternalServerError, response.ToString(), body);
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TModel>(json);
        }

        #endregion
    }
}
