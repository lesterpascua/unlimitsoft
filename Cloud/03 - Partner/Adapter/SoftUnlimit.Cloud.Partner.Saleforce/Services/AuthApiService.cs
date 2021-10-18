using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SoftUnlimit.Cloud.Partner.Saleforce.Sender.Configuration;
using SoftUnlimit.Web.Client;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.Cloud.Partner.Saleforce.Sender.Services
{
    /// <inheritdoc />
    public class AuthApiService : BaseApiService, IAuthApiService
    {
        private AccessTokenModel _accessToken;
        private DateTime _expirationDate;
        private readonly FormUrlEncodedContent _httpContent;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="options"></param>
        public AuthApiService(IApiClient client, IOptions<SaleforceOption> options)
            : base(client, slidingExpiration: null)
        {
            var nvc = new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("client_id", options.Value.ClientId),
                new KeyValuePair<string, string>("client_secret", options.Value.Secret),
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", options.Value.UserName),
                new KeyValuePair<string, string>("password", options.Value.Password),
            };

            EventName = options.Value.EventName;
            _httpContent = new FormUrlEncodedContent(nvc);
            _httpContent.Headers.Add("X-PrettyPrint", "1");

            _accessToken = null;
        }

        /// <inheritdoc />
        public string EventName { get; }
        /// <inheritdoc />
        public Task<string> GetTokenAsync(CancellationToken ct = default) => ThreadSafeGetToken(ct);

        #region Private Methods
        private async Task<string> ThreadSafeGetToken(CancellationToken ct)
        {
            if (_accessToken != null && DateTime.UtcNow < _expirationDate)
                return _accessToken.Token;

            await _semaphore.WaitAsync(ct);
            try
            {
                if (_accessToken != null && DateTime.UtcNow < _expirationDate)
                    return _accessToken.Token;

                HttpStatusCode code;
                (_accessToken, code) = await ApiClient
                    .SendAsync<AccessTokenModel>(HttpMethod.Post, "services/oauth2/token", message => message.Content = _httpContent, ct: ct);

                // TODO: review the real expiration time in saleforce.
                _expirationDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(_accessToken.IssuedAt).AddMinutes(55);
                return _accessToken.Token;
            }
            finally
            {
                _semaphore.Release();
            }
        }
        #endregion

        #region Nested Classes

        /// <summary>
        /// 
        /// </summary>
        private class AccessTokenModel
        {
            /// <summary>
            /// 
            /// </summary>
            [JsonProperty("access_token")]
            [JsonPropertyName("access_token")]
            public string Token { get; set; }
            [JsonProperty("instance_url")]
            [JsonPropertyName("instance_url")]
            public string InstanceUrl { get; set; }
            /// <summary>
            /// 
            /// </summary>
            [JsonProperty("id")]
            [JsonPropertyName("id")]
            public string Id { get; set; }
            /// <summary>
            /// 
            /// </summary>
            [JsonProperty("token_type")]
            [JsonPropertyName("token_type")]
            public string TokenType { get; set; }
            /// <summary>
            /// 
            /// </summary>
            [JsonProperty("issued_at")]
            [JsonPropertyName("issued_at")]
            public long IssuedAt { get; set; }
            /// <summary>
            /// 
            /// </summary>
            [JsonProperty("signature")]
            [JsonPropertyName("signature")]
            public string Signature { get; set; }
        }
        #endregion
    }
}
