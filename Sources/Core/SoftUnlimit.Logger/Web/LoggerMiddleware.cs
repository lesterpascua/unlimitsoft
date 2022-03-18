using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SoftUnlimit.Logger.Logging;
using System.Threading.Tasks;

namespace SoftUnlimit.Logger.Web
{
    /// <summary>
    /// Add logger correlation and traceId. Can add custom properties.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LoggerMiddleware<T> where T : LoggerContext, new()
    {
        private readonly RequestDelegate _next;
        private readonly ILoggerContextAccessor _accessor;
        private readonly ILogger<LoggerMiddleware<T>> _logger;

        /// <summary>
        /// Header asociate to correlation id if the header is not present use TraceId
        /// </summary>
        public const string CorrelationHeader = "X-Correlation-ID";


        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        /// <param name="accessor"></param>
        /// <param name="logger"></param>
        public LoggerMiddleware(RequestDelegate next, ILoggerContextAccessor accessor, ILogger<LoggerMiddleware<T>> logger)
        {
            _next = next;
            _accessor = accessor;
            _logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = context.TraceIdentifier;
            if (context.Request.Headers.TryGetValue(CorrelationHeader, out var correlationHeader))
                correlationId = correlationHeader;

            LoggerUtility.SafeUpdateContext<T>(
                _accessor,
                loggerContext =>
                {
                    loggerContext.CorrelationId = correlationId;
                    loggerContext.TraceId = context.TraceIdentifier;
                }
            );
            // Log the asociation to historical debuging process
            if (context.TraceIdentifier != correlationId)
                _logger.AssociateTraceWithCorrelation(context.TraceIdentifier, correlationId);

            // Call the next delegate/middleware in the pipeline.
            await _next(context);
        }
    }
}