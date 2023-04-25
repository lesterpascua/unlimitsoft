namespace UnlimitSoft;


/// <summary>
/// Define constant used in the UnlimitSoft system
/// </summary>
public static class SysContants
{
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
    public const string HeaderTrace = "X-Trace-ID";
    /// <summary>
    /// Correlation identifier.
    /// </summary>
    public const string HeaderCorrelation = "X-Correlation-ID";
}
