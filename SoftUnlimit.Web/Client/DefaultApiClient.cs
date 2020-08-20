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
    public abstract class DefaultApiClient : IApiClient
    {
        private readonly string _baseUrl;
        private readonly HttpClient _httpClient;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="baseUrl"></param>
        public DefaultApiClient(HttpClient httpClient, string baseUrl)
        {
            this._baseUrl = baseUrl;
            this._httpClient = httpClient;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="method"></param>
        /// <param name="uri"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<TModel> SendAsync<TModel>(HttpMethod method, string uri, object model = null)
        {
            string jsonContent = null;
            string completeUri = string.Concat(this._baseUrl, uri);

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
            if (jsonContent != null)
                httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            await this.BeforeSendRequestAsync(method, uri);
            return await this.SendWithContentAsync<TModel>(_httpClient, method, completeUri, httpContent);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="method"></param>
        /// <param name="uri"></param>
        /// <param name="token"></param>
        /// <param name="fileName"></param>
        /// <param name="streams"></param>
        /// <param name="qs"></param>
        /// <returns></returns>
        public async Task<TModel> UploadAsync<TModel>(HttpMethod method, string uri, string fileName, IEnumerable<Stream> streams, object qs = null)
        {
            if (method == HttpMethod.Get)
                throw new NotSupportedException("Get method don't allow upload image.");

            string completeUri = string.Concat(_baseUrl, uri);
            var content = new MultipartFormDataContent("Uploading... " + DateTime.Now.ToString(CultureInfo.InvariantCulture));
            foreach (var stream in streams)
                content.Add(new StreamContent(stream), fileName, fileName);

            if (qs != null)
                completeUri += string.Concat('?', await ObjectUtils.ToQueryString(qs));

            await BeforeSendRequestAsync(method, uri);
            return await this.SendWithContentAsync<TModel>(_httpClient, method, completeUri, content);
        }

        /// <summary>
        /// Performs operations before request.
        /// </summary>
        /// <param name="method"></param>
        /// <param name="uri"></param>
        /// <returns></returns>
        protected abstract Task BeforeSendRequestAsync(HttpMethod method, string uri);

        #region Private Methods

        private async Task<TModel> SendWithContentAsync<TModel>(HttpClient httpClient, HttpMethod method, string completeUri, HttpContent content)
        {
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
