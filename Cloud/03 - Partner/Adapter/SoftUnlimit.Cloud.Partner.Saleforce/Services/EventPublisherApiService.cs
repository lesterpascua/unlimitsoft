using Newtonsoft.Json;
using SoftUnlimit.Cloud.Partner.Saleforce.Sender.Model;
using SoftUnlimit.Json;
using SoftUnlimit.Web.Client;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.Cloud.Partner.Saleforce.Sender.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class EventPublisherApiService : BaseApiService, IEventPublisherApiService
    {
        private readonly string _url;
        private readonly IAuthApiService _authService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="authService"></param>
        public EventPublisherApiService(IApiClient client, IAuthApiService authService)
            : base(client, slidingExpiration: null)
        {
            _authService = authService;
            _url = $"/services/data/v51.0/sobjects/{_authService.EventName}";
        }

        /// <inheritdoc />
        public async Task<(PublishStatus, HttpStatusCode)> PublishAsync(EventSignature e, CancellationToken ct)
        {
            string token = await _authService.GetTokenAsync(ct);
            void setup(HttpRequestMessage message) => message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var body = JsonUtility.Cast<SfRecord>(e.Body);
            var model = new Internal
            {
                Body = JsonConvert.SerializeObject(e.Body),
                Name = e.Name,
                ExternalId = e.SourceId.ToString(),
                SfRecordId = body.SfRecordId
            };
            return await ApiClient.SendAsync<PublishStatus>(HttpMethod.Post, _url, setup, model, ct);
        }

        private record SfRecord(string SfRecordId);
        private sealed class Internal
        {
            [JsonProperty("Body__c")]
            [JsonPropertyName("Body__c")]
            public string Body { get; set; }

            [JsonProperty("Name__c")]
            [JsonPropertyName("Name__c")]
            public string Name { get; set; }

            [JsonProperty("ExternalId__c")]
            [JsonPropertyName("ExternalId__c")]
            public string ExternalId { get; set; }

            [JsonProperty("SfRecordId__c")]
            [JsonPropertyName("SfRecordId__c")]
            public string SfRecordId { get; set; }
        }
    }
}
