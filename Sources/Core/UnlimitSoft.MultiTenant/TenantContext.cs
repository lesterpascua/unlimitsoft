namespace UnlimitSoft.MultiTenant;


/// <summary>
/// Get context information used to build the tenant
/// </summary>
public abstract class TenantContext
{
    /// <summary>
    /// Tenant key
    /// </summary>
    public Tenant? Tenant { get; set; }
}
