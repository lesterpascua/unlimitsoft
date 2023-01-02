using System;

namespace UnlimitSoft.MultiTenant;


/// <summary>
/// Allow access to the root tenant
/// </summary>
public interface IRootTenantServiceProvider
{
    /// <summary>
    /// Get root service provider.
    /// </summary>
    /// <returns></returns>
    IServiceProvider? GetProvider();
}
internal sealed class RootTenantServiceProvider : IRootTenantServiceProvider
{
    internal IServiceProvider? _root;

    /// <inheritdoc />
    public IServiceProvider? GetProvider() => _root;
}