using UnlimitSoft.MultiTenant;
using UnlimitSoft.MultiTenant.Store;

namespace UnlimitSoft.WebApi.MultiTenant.Sources.MultiTenant;


public class MyTenantStorage : ITenantStore
{
    private readonly IReadOnlyDictionary<string, Tenant> _tenants;

    public MyTenantStorage()
    {
        _tenants = new Dictionary<string, Tenant>
        {
            ["tenant1"] = new Tenant(Guid.Parse("00000000-0000-0000-0000-000000000001"), "tenant1"),
            ["tenant2"] = new Tenant(Guid.Parse("00000000-0000-0000-0000-000000000002"), "tenant2"),
            ["tenant3"] = new Tenant(Guid.Parse("00000000-0000-0000-0000-000000000003"), "tenant3"),
        };
    }

    public Tenant? GetTenant(string identifier)
    {
        if (_tenants.TryGetValue(identifier, out var tenant))
            return tenant;
        return null;
    }
}
