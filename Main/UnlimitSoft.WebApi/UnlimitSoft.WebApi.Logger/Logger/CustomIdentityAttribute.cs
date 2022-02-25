using Microsoft.AspNetCore.Mvc.Filters;
using SoftUnlimit.Logger;

namespace UnlimitSoft.WebApi.Logger.Logger;

public class CustomIdentityAttribute : ActionFilterAttribute
{
    private readonly ILoggerContextAccessor _loggerAccessor;

    public CustomIdentityAttribute(ILoggerContextAccessor loggerAccessor)
    {
        _loggerAccessor = loggerAccessor;
    }
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        LoggerUtility.SafeUpdateContext<MyLoggerContext>(
            _loggerAccessor,
            loggerContext =>
            {
                loggerContext.IdentityId = $"My Customer Identity = {DateTime.UtcNow}";
            }
        );
    }
}
