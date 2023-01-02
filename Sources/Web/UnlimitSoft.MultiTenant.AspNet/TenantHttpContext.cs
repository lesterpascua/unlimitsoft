using Microsoft.AspNetCore.Http;

namespace UnlimitSoft.MultiTenant.AspNet;


/// <summary>
/// Create a tenant context when is a http request.
/// </summary>
public sealed class TenantHttpContext : TenantContext
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    public TenantHttpContext(HttpContext context)
    {
        Context = context;
    }

    /// <summary>
    /// Http context asociate to the current thread.
    /// </summary>
    public HttpContext Context { get; }
}
