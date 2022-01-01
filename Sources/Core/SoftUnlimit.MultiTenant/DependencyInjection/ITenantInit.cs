namespace SoftUnlimit.MultiTenant.DependencyInjection
{
    /// <summary>
    /// 
    /// </summary>
    public interface ITenantInit<T>
    {
        /// <summary>
        /// Initilization tenant operations.
        /// </summary>
        /// <param name="tenant"></param>
        void Init(T tenant);
    }
}
