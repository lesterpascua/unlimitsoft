namespace UnlimitSoft.MultiTenant.DependencyInjection
{
    /// <summary>
    /// 
    /// </summary>
    public interface ITenantConfigure
    {
        /// <summary>
        /// Initilization tenant operations.
        /// </summary>
        /// <param name="tenant"></param>
        void Configure(Tenant tenant);
    }
}
