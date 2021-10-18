using CometD.NetCore.Salesforce.Messaging;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace SoftUnlimit.Cloud.Partner.Domain.Handler.Services.Saleforce
{
    public sealed class SalesforceCloudPayload: MessagePayload
    {
        [JsonProperty("Body__c")]
        [JsonPropertyName("Body__c")]
        public string Body { get; set; } = string.Empty;
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
