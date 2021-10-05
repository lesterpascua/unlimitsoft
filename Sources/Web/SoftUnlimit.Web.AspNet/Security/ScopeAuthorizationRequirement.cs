using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoftUnlimit.Web.AspNet.Security
{
    /// <summary>
    /// 
    /// </summary>
    public class ScopeAuthorizationRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<string> RequiredScopes { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="requiredScopes"></param>
        public ScopeAuthorizationRequirement(IEnumerable<string> requiredScopes = null)
        {
            RequiredScopes = requiredScopes;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class ScopeAuthorizationRequirementHandler : AuthorizationHandler<ScopeAuthorizationRequirement>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="requirement"></param>
        /// <returns></returns>
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ScopeAuthorizationRequirement requirement)
        {
            var scopes = context.User?.Claims
                .Where(p => string.Equals(p.Type, ScopePolicyProvider.ClaimName, StringComparison.CurrentCultureIgnoreCase))
                .Select(s => s.Value);

            if (requirement.RequiredScopes.All(s => scopes.Contains(s)))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }

    }
}
