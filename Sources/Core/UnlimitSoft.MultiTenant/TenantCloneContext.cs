namespace UnlimitSoft.MultiTenant
{
    /// <summary>
    /// 
    /// </summary>
    public class TenantCloneContext : TenantContext
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenant"></param>
        public TenantCloneContext(Tenant tenant)
        {
            Tenant = tenant;
        }

        /// <summary>
        /// Tenant key
        /// </summary>
        public Tenant Tenant { get; }
    }
}
