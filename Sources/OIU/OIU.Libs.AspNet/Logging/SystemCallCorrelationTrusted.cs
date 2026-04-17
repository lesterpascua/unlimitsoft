using Microsoft.AspNetCore.Http;
using UnlimitSoft.Logger.AspNet;

namespace OIU.Libs.AspNet.Logging;


/// <summary>
/// Only take the correlation in internal sys calls
/// </summary>
public sealed class SystemCallCorrelationTrusted : ICorrelationTrusted
{
    /// <inheritdoc />
    public bool IsTrustedRequest(HttpContext context)
    {
        var path = context.Request.Path.Value;
        return path?.Contains("/sys/", StringComparison.InvariantCultureIgnoreCase) == true;
    }
}
