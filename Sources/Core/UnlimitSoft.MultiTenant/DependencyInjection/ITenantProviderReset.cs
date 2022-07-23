namespace SoftUnlimit.MultiTenant.DependencyInjection
{
    /// <summary>
    /// Reset the provider of some specific tenant.
    /// </summary>
    public interface ITenantProviderReset
    {
        /// <summary>
        /// Remove the provider for a tenant in the current context so the next time the system will recreate.
        /// </summary>
        void Reset();
    }
    internal class TenantProviderReset : ITenantProviderReset
    {
        private readonly Tenant _tenant;
        private readonly TenantServiceProvider _root;

        public TenantProviderReset(Tenant tenant, TenantServiceProvider root)
        {
            _root = root;
            _tenant = tenant;
        }

        public void Reset() => _root.Clean(_tenant);
    }
}