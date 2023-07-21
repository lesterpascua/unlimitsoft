namespace UnlimitSoft;


/// <summary>
/// Define constant used in the UnlimitSoft system
/// </summary>
public static class Constants
{
    /// <summary>
    /// Name of the event this is use to resolve the type
    /// </summary>
    public const string HeaderEventName = "X-Event";
    /// <summary>
    /// Indicate if the message has envelop or not
    /// </summary>
    public const string HeaderHasEnvelop = "X-Has-Envelop";

    /// <summary>
    /// Name of the trace in the logger context
    /// </summary>
    public const string LogContextTraceId = "TraceId";
    /// <summary>
    /// Name of the correlation in the logger context
    /// </summary>
    public const string LogContextCorrelationId = "CorrelationId";

    /// <summary>
    /// Trace identifier
    /// </summary>
    public const string HeaderApiKey = "X-API-KEY";
    /// <summary>
    /// Trace identifier
    /// </summary>
    public const string HeaderTrace = "X-Trace-ID";
    /// <summary>
    /// Correlation identifier.
    /// </summary>
    public const string HeaderCorrelation = "X-Correlation-ID";

    /// <summary>
    /// 
    /// </summary>
    public const string HttpContextTenantKey = "TenantKey";
}
