using Microsoft.Extensions.Logging;
using System;

namespace SoftUnlimit.Logger.Logging
{
    /// <summary>
    /// 
    /// </summary>
    public static class ILoggerDefinitionExtensions
    {
        private static readonly Action<ILogger, string, string?, Exception?> __AssociateTraceWithCorrelation = LoggerMessage.Define<string, string?>(LogLevel.Information, 0, "Associate {Trace} with {Correlation}");

        /// <summary>
        /// Associate {Trace} with {Correlation}
        /// </summary>
        /// <param name="this"></param>
        /// <param name="trace"></param>
        /// <param name="correlation"></param>
        public static void AssociateTraceWithCorrelation(this ILogger @this, string trace, string? correlation) => __AssociateTraceWithCorrelation(@this, trace, correlation, null);
    }
}