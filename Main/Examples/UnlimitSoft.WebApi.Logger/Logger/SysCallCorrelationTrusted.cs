using UnlimitSoft.Logger.AspNet;

namespace UnlimitSoft.WebApi.Logger.Logger;

public class SysCallCorrelationTrusted : ICorrelationTrusted
{
    public bool IsTrustedRequest(HttpContext context)
    {
        var path = context.Request.Path.Value;
        return path?.EndsWith("syscall", StringComparison.InvariantCultureIgnoreCase) == true;
    }
}
