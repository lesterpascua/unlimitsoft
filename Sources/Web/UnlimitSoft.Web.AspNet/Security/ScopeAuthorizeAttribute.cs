using Microsoft.AspNetCore.Authorization;

namespace UnlimitSoft.Web.AspNet.Security
{
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
        /// 
        /// </summary>
        /// <param name="scope"></param>
        public ScopeAuthorizeAttribute(string scope) => Scope = scope;

        /// <summary>
        /// Scopes required one per attribute
        /// </summary>
        public string Scope { get => Policy[ScopePolicyProvider.ScopePolicyPrefix.Length..]; set => Policy = $"{ScopePolicyProvider.ScopePolicyPrefix}{value}"; }
    }
}
