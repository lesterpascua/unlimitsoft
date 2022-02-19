using Serilog.Core;
using Serilog.Events;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace SoftUnlimit.Logger.Enricher
{
    /// <summary>
    /// 
    /// </summary>
    public class LoggerContextEnricher : ILogEventEnricher
    {
        private static PropertyInfo[]? _cache;
        private readonly ILoggerContextAccessor _accesor;

        /// <summary>
        /// Correlation Property name
        /// </summary>
        public const string Name = "CorrelationId";


        /// <summary>
        /// 
        /// </summary>
        public LoggerContextEnricher()
            : this(new LoggerContextAccessor())
        { }
        /// <summary>
        /// 
        /// </summary>
        internal LoggerContextEnricher(ILoggerContextAccessor accesor)
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
            var context = _accesor.Context;
            var correlationId = GetCorrelationId(logEvent, context);
            if (correlationId is not null)
            {
                var correlationIdProperty = new LogEventProperty(Name, new ScalarValue(correlationId));
                logEvent.AddOrUpdateProperty(correlationIdProperty);
            }
            if (context is null)
                return;

            var type = context.GetType();
            if (type == typeof(LoggerContext))
                return;

            if (_cache is null)
            {
                var properties = context.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var tmp = properties.Where(p => p.Name != nameof(LoggerContext.TraceId) && p.Name != nameof(LoggerContext.CorrelationId)).ToArray();
                Interlocked.CompareExchange(ref _cache, tmp, null);
            }
            foreach (var property in _cache)
            {
                var value = property.GetValue(context);
                if (value is null)
                    continue;
                var logEventProperty = new LogEventProperty(property.Name, new ScalarValue(value));
                logEvent.AddOrUpdateProperty(logEventProperty);
            }
        }

        #region Private Methods
        private static string? GetCorrelationId(LogEvent logEvent, LoggerContext? context)
        {
            if (context is not null)
                return context.CorrelationId;
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
