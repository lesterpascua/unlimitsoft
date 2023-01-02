using System.Threading;

namespace UnlimitSoft.MultiTenant;


/// <summary>
/// Contain information to build a tenant. This information is attached to the executing thread.
/// </summary>
public interface ITenantContextAccessor
{
    /// <summary>
    /// 
    /// </summary>
    TenantContext? GetContext();
}
/// <summary>
/// Default implementation of <see cref="ITenantContextAccessor"/>
/// </summary>
public sealed class TenantContextAccessor : ITenantContextAccessor
{
    private static readonly AsyncLocal<ContextHolder> _current = new();


    /// <inheritdoc />
    public TenantContext? GetContext() => _current.Value?.Context;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    public void SetContext(TenantContext value)
    {
        var holder = _current.Value;
        // Clear current HttpContext trapped in the AsyncLocals, as its done.
        if (holder is not null)
            holder.Context = null;

        // Use an object indirection to hold the CorrelationContext in the AsyncLocal, so it can be cleared in all ExecutionContexts when its cleared.
        if (value is not null)
            _current.Value = new ContextHolder { Context = value };
    }


    #region Nested Classes
    private class ContextHolder
    {
        public TenantContext? Context;
    }
    #endregion
}
