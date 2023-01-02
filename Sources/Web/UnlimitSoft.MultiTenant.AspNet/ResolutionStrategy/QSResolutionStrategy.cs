using UnlimitSoft.MultiTenant.AspNet;

namespace UnlimitSoft.MultiTenant.ResolutionStrategy;


/// <summary>
/// Resolve using query string url tenant=identifier
/// </summary>
public sealed class QSResolutionStrategy : ITenantResolutionStrategy
{
    private readonly ITenantContextAccessor _accessor;
    private readonly string _qsKey;

    /// <summary>
    /// Key in the query string to search
    /// </summary>
    public const string QSKey = "tenant";

    /// <summary>
    /// 
    /// </summary>
    /// <param name="accessor"></param>
    /// <param name="qsKey">Query string key where the tenant info will be get.</param>
    public QSResolutionStrategy(ITenantContextAccessor accessor, string qsKey = QSKey)
    {
        _accessor = accessor;
        _qsKey = qsKey;
    }

    /// <summary>
    /// Get the tenant identifier
    /// </summary>
    /// <param name="tenant"></param>
    /// <returns></returns>
    public string? GetKey(out Tenant? tenant)
    {
        tenant = null;
        var context = _accessor.GetContext() as TenantHttpContext;
        return context?.Context?.Request.Query[_qsKey];
    }
}