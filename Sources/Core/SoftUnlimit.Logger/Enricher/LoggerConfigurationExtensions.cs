using Serilog;
using Serilog.Configuration;
using System;

namespace SoftUnlimit.Logger.Enricher
{
    /// <summary>
    /// 
    /// </summary>
    public static class LoggerConfigurationExtensions
    {
        /// <summary>
        /// Define a custom properties to log in every message
        /// </summary>
        /// <param name="enrichmentConfiguration"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static LoggerConfiguration WithLoggerContext(this LoggerEnrichmentConfiguration enrichmentConfiguration) 
        {
            if (enrichmentConfiguration == null)
                throw new ArgumentNullException(nameof(enrichmentConfiguration));
            return enrichmentConfiguration.With<LoggerContextEnricher>();
        }
    }
}
