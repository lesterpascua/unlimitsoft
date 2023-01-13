using Microsoft.AspNetCore.Http;

namespace UnlimitSoft.Logger.AspNet;


/// <summary>
/// 
/// </summary>
public interface ICorrelationTrusted
{
    /// <summary>
    /// Check if the request is trusted if is the case take the correlation from request only if is supplied
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    bool IsTrustedRequest(HttpContext context);
}
