using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace SoftUnlimit.Logger
{
    /// <summary>
    /// Use this filter to in web context assing the Correlation context for serilog.
    /// </summary>
    public class CorrelationContextAttribute : ActionFilterAttribute
    {
        private readonly ICorrelationContext _context;
        private readonly ICorrelationContextAccessor _accessor;
        private readonly ILogger<CorrelationContextAttribute> _logger;

        /// <summary>
        /// 
        /// </summary>
        public const string CorrelationHeader = "X-Correlation-ID";


        /// <summary>
        /// 
        /// </summary>
        /// <param name="accessor"></param>
        /// <param name="context"></param>
        /// <param name="logger"></param>
        public CorrelationContextAttribute(ICorrelationContextAccessor accessor, ICorrelationContext context, ILogger<CorrelationContextAttribute> logger)
        {
            _accessor = accessor;
            _context = context;
            _logger = logger;
        }

        /// <inheritdoc />
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var correlationId = context.HttpContext.TraceIdentifier;
            if (context.HttpContext.Request.Headers.TryGetValue(CorrelationHeader, out var correlationHeader))
                correlationId = correlationHeader;

            Utility.SafeUpdateCorrelationContext(_accessor, _context, correlationId);

            _logger.LogInformation("Associate {Trace} with {Correlation}", context.HttpContext.TraceIdentifier, _context.CorrelationId);
        }
    }
}
