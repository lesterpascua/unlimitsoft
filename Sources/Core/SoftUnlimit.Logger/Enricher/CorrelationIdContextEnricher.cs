using Serilog.Core;
using Serilog.Events;
using System.IO;

namespace SoftUnlimit.Logger.Enricher
{
    /// <summary>
    /// 
    /// </summary>
    public class CorrelationIdContextEnricher : ILogEventEnricher
    {
        private const string Name = "CorrelationId";
        private readonly ICorrelationContextAccessor _accesor;


        /// <summary>
        /// 
        /// </summary>
        public CorrelationIdContextEnricher()
            : this(new DefaultCorrelationContextAccessor())
        { }
        /// <summary>
        /// 
        /// </summary>
        internal CorrelationIdContextEnricher(ICorrelationContextAccessor accesor)
        {
            _accesor = accesor;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logEvent"></param>
        /// <param name="propertyFactory"></param>
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (!logEvent.Properties.TryGetValue("RequestId", out var requestId) && _accesor.Context is null)
                return;

            var correlationId = _accesor.Context?.CorrelationId;
            if (correlationId is null)
                correlationId = requestId.ToString().Replace("\"", string.Empty);
            var correlationIdProperty = new LogEventProperty(Name, new ScalarValue(correlationId));
            logEvent.AddOrUpdateProperty(correlationIdProperty);
        }
    }
}
