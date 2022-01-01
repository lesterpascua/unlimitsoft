using SoftUnlimit.MultiTenant.AspNet;

namespace SoftUnlimit.MultiTenant.ResolutionStrategy;

/// <summary>
/// Resolve using query string url tenant=identifier
/// </summary>
public class QSResolutionStrategy : ITenantResolutionStrategy
{
    private readonly ITenantContextAccessor _accessor;

    /// <summary>
    /// Key in the query string to search
    /// </summary>
    public const string QSKey = "tenant";

    /// <summary>
    /// 
    /// </summary>
    /// <param name="accessor"></param>
    public QSResolutionStrategy(ITenantContextAccessor accessor)
    {
        _accessor = accessor;
    }

    /// <summary>
    /// Get the tenant identifier
    /// </summary>
    /// <returns></returns>
    public string? GetIdentifier()
    {
        var context = _accessor.GetContext() as TenantHttpContext;
        return context?.Context?.Request.Query[QSKey];
    }
}