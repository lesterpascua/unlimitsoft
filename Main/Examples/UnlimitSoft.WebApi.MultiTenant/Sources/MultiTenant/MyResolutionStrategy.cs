using UnlimitSoft.MultiTenant;
using UnlimitSoft.MultiTenant.ResolutionStrategy;

namespace UnlimitSoft.WebApi.MultiTenant.Sources.MultiTenant;


/// <summary>
/// 
/// </summary>
public sealed class MyResolutionStrategy : ITenantResolutionStrategy
{
    private readonly ITenantContextAccessor _accessor;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="accessor"></param>
    public MyResolutionStrategy(ITenantContextAccessor accessor)
    {
        _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
    }

    public string? GetKey(out Tenant? tenant)
    {
        tenant = null;
        return null;

        var context = _accessor.GetContext();
        tenant = new Tenant(Guid.Empty, "test");
        return tenant.Key;
    }
}
