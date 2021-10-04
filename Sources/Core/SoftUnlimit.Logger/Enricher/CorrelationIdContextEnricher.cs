using Serilog.Core;
using Serilog.Events;

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
            if (_accesor.Context == null)
                return;

            var traceContext = _accesor.Context;
            var correlationId = traceContext.CorrelationId;
            var correlationIdProperty = new LogEventProperty(Name, new ScalarValue(correlationId));
            logEvent.AddOrUpdateProperty(correlationIdProperty);
        }
    }
}
