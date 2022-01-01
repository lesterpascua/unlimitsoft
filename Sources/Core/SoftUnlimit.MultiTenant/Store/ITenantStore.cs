using System.Threading.Tasks;

namespace SoftUnlimit.MultiTenant.Store;

/// <summary>
/// Provide a way to access to a tenant store.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ITenantStore<T> where T : Tenant
{
    /// <summary>
    /// Get tenant asociate with some identifier.
    /// </summary>
    /// <param name="identifier"></param>
    /// <returns></returns>
    ValueTask<T> GetTenantAsync(string identifier);
}