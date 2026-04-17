using OIU.Libs.Security;
using UnlimitSoft.Json;

namespace OIU.Libs.Web;


/// <summary>
/// Add helper method to add autentication
/// </summary>
public static class HttpRequestMessageExtensions
{
    /// <summary>
    /// Add correlation to the request
    /// </summary>
    /// <param name="this"></param>
    /// <param name="correlationId"></param>
    public static void AddCorrelationId(this HttpRequestMessage @this, string? correlationId)
    {
        if (correlationId is not null)
            @this.Headers.TryAddWithoutValidation(UnlimitSoft.Constants.HeaderCorrelation, correlationId);
    }
    /// <summary>
    /// Add auth header in the request with system authority
    /// </summary>
    /// <param name="this"></param>
    /// <param name="serializer"></param>
    /// <param name="apiKey"></param>
    /// <param name="correlationId"></param>
    public static void AddApiKeyHeader(this HttpRequestMessage @this, IJsonSerializer serializer, string apiKey, string? correlationId = null)
    {
        @this.Headers.TryAddWithoutValidation(UnlimitSoft.Constants.HeaderApiKey, apiKey);
        @this.Headers.TryAddWithoutValidation(Constants.AuthorizationHeader, Identity.GetSystem().ToAuthHeader(serializer));
        @this.AddCorrelationId(correlationId);
    }
    /// <summary>
    /// Add auth header in the request with custom identity
    /// </summary>
    /// <param name="this"></param>
    /// <param name="serializer"></param>
    /// <param name="identity"></param>
    /// <param name="apiKey"></param>
    /// <param name="correlationId"></param>
    public static void AddApiKeyHeader(this HttpRequestMessage @this, IJsonSerializer serializer, Identity? identity, string apiKey, string? correlationId = null)
    {
        identity ??= Identity.GetSystem();

        @this.Headers.TryAddWithoutValidation(UnlimitSoft.Constants.HeaderApiKey, apiKey);
        @this.Headers.TryAddWithoutValidation(Constants.AuthorizationHeader, identity.ToAuthHeader(serializer));
        @this.AddCorrelationId(correlationId);
    }
}
