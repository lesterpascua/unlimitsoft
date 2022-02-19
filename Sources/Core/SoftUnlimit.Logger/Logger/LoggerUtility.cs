using System;

namespace SoftUnlimit.Logger
{
    /// <summary>
    /// 
    /// </summary>
    public static class LoggerUtility
    {
        /// <summary>
        /// Update the current context with some in the parameter.
        /// </summary>
        /// <param name="accessor"></param>
        /// <param name="traceId"></param>
        /// <param name="correlationId"></param>
        public static void SafeUpdateCorrelation(ILoggerContextAccessor accessor, string traceId, string correlationId)
        {
            var context = accessor.Context;
            if (context is null)
                context = new LoggerContext();

            context.TraceId = traceId;
            context.CorrelationId = correlationId;
        }
        /// <summary>
        /// Update the current context with some in the parameter.
        /// </summary>
        /// <param name="accessor"></param>
        /// <param name="action"></param>
        public static void SafeUpdateContext<T>(ILoggerContextAccessor accessor, Action<T> action) where T : LoggerContext, new()
        {
            if (accessor.Context is not T aux)
                accessor.Context = aux = new T();
            action(aux);
        }
    }
}
