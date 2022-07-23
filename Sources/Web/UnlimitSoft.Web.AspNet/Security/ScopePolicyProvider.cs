using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace UnlimitSoft.Web.AspNet.Security
{
    /// <summary>
    /// 
    /// </summary>
    public class ScopePolicyProvider : IAuthorizationPolicyProvider
    {
        private readonly DefaultAuthorizationPolicyProvider _defaultProvider;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        public ScopePolicyProvider(IOptions<AuthorizationOptions> options)
        {
            // ASP.NET Core only uses one authorization policy provider, so if the custom implementation
            // doesn't handle all policies it should fall back to an alternate provider.
            _defaultProvider = new DefaultAuthorizationPolicyProvider(options);
        }

        /// <inheritdoc />
        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _defaultProvider.GetDefaultPolicyAsync();
        /// <inheritdoc />
        public Task<AuthorizationPolicy> GetFallbackPolicyAsync() => _defaultProvider.GetFallbackPolicyAsync();

        /// <inheritdoc />
        public async Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            if (policyName.StartsWith(ScopePolicyPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var scope = policyName[ScopePolicyPrefix.Length..];

                var policy = new AuthorizationPolicyBuilder();
                policy.AddRequirements(new ScopeAuthorizationRequirement(new string[] { scope }));

                return await Task.FromResult(policy.Build());
            }

            return await _defaultProvider.GetPolicyAsync(policyName);
        }


        /// <summary>
        /// Name of the claim assing to the scope.
        /// </summary>
        public static string ClaimName { get; set; } = "scope";
        /// <summary>
        /// Policy prefix.
        /// </summary>
        public static string ScopePolicyPrefix { get; set; } = "AuthorizeScope_";
    }
}
