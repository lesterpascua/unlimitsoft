using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace UnlimitSoft.Web.AspNet.Security;


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
    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _defaultProvider.GetFallbackPolicyAsync();

    /// <inheritdoc />
    public async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(ScopePolicyPrefix, StringComparison.OrdinalIgnoreCase))
        {
#if NET9_0_OR_GREATER
            var value = policyName.AsSpan()[ScopePolicyPrefix.Length..];

            var data = value.Split(',');
            var length = value.Count(',') + 1;

            var scopes = new string[length];
            for (var i = 0; i < length; i++)
            {
                data.MoveNext();
                scopes[i] = value[data.Current].ToString();
            }
#else
            var value = policyName[ScopePolicyPrefix.Length..];
            var scopes = value.Split(',', StringSplitOptions.RemoveEmptyEntries);
#endif

            var policy = new AuthorizationPolicyBuilder();
            policy.AddRequirements(new ScopeAuthorizationRequirement(scopes));

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
