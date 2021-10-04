using Serilog;
using Serilog.Configuration;
using System;

namespace SoftUnlimit.Logger.Enricher
{
    /// <summary>
    /// 
    /// </summary>
    public static class CorrelationIdLoggerConfigurationExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="enrichmentConfiguration"></param>
        /// <returns></returns>
        public static LoggerConfiguration WithCorrelationIdContext(this LoggerEnrichmentConfiguration enrichmentConfiguration)
        {
            if (enrichmentConfiguration == null)
                throw new ArgumentNullException(nameof(enrichmentConfiguration));
            return enrichmentConfiguration.With<CorrelationIdContextEnricher>();
        }
    }
}
