using System.Threading.Tasks;

namespace SoftUnlimit.MultiTenant.Store;

/// <summary>
/// Provide a way to access to a tenant store.
/// </summary>
public interface ITenantStore
{
    /// <summary>
    /// Get tenant asociate with some identifier.
    /// </summary>
    /// <param name="identifier"></param>
    /// <returns></returns>
    Tenant GetTenant(string identifier);
}