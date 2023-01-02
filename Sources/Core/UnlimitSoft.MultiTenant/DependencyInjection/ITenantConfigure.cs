namespace UnlimitSoft.MultiTenant.DependencyInjection;


/// <summary>
/// Allow define an initialization of the tenant. 
/// </summary>
public interface ITenantConfigure
{
    /// <summary>
    /// Initilization tenant operations. This method is execute only the first time the tenant operation is called and use as initialization 
    /// is the same Configure(WebApplication app) in asp.net standard flow.
    /// </summary>
    /// <param name="tenant"></param>
    void Configure(Tenant tenant);
}
