using Microsoft.AspNetCore.Authorization;

namespace UnlimitSoft.Web.AspNet.Security;


/// <summary>
/// Include authorization filter based in scopes.
/// </summary>
public class ScopeAuthorizeAttribute : AuthorizeAttribute
{
    /// <summary>
    /// 
    /// </summary>
    public ScopeAuthorizeAttribute() { }

    /// <summary>
    /// Scopes required one per attribute
    /// </summary>
    public string? Scope
    {
        get => Policy?[ScopePolicyProvider.ScopePolicyPrefix.Length..];
        set => Policy = value is not null ? $"{ScopePolicyProvider.ScopePolicyPrefix}{value}" : null;
    }
}
