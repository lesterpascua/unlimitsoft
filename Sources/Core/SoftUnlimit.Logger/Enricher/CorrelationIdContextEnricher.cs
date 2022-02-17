using Serilog.Core;
using Serilog.Events;
using System.IO;
using System.Linq;

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
            var correlationId = GetCorrelationId(logEvent);
            if (correlationId is not null)
            {
                var correlationIdProperty = new LogEventProperty(Name, new ScalarValue(correlationId));
                logEvent.AddOrUpdateProperty(correlationIdProperty);
            }
        }

        #region Private Methods
        private string? GetCorrelationId(LogEvent logEvent)
        {
            if (_accesor.Context is not null)
                return _accesor.Context.CorrelationId;
            if (logEvent.Properties.TryGetValue("RequestId", out var requestId))
                return requestId.ToString().Replace("\"", string.Empty);
            if (logEvent.Properties.TryGetValue("Event", out var @event) && @event is StructureValue structureValue)
            {
                var correlationId = structureValue.Properties.FirstOrDefault(p => p.Name == Name);
                if (correlationId is not null)
                    return correlationId.Value.ToString().Replace("\"", string.Empty);
            }
            return null;
        }
        #endregion
    }
}
