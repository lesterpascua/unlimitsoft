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
    internal class TenantProviderReset<T> : ITenantProviderReset where T : Tenant
    {
        private readonly T _tenant;
        private readonly TenantServiceProvider<T> _root;

        public TenantProviderReset(T tenant, TenantServiceProvider<T> root)
        {
            _root = root;
            _tenant = tenant;
        }

        public void Reset() => _root.Clean(_tenant);
    }
}