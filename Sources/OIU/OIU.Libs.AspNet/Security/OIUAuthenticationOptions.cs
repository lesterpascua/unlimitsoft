using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OIU.Libs.Security;
using System.Security.Claims;
using UnlimitSoft.Json;
using UnlimitSoft.Web.AspNet.Security.Authentication;

namespace OIU.Libs.AspNet.Security;


/// <summary>
/// 
/// </summary>
public class OIUAuthenticationOptions : ApiKeyAuthenticationOptions
{
    /// <inheritdoc />
    protected override ValueTask<IEnumerable<Claim>?> CreateClaims(IServiceProvider provider, HttpRequest httpRequest, string? apiKey)
    {
        var serializer = provider.GetRequiredService<IJsonSerializer>();
        var json = Identity.FromAuthHeader(httpRequest.Headers, out var _);
        if (json is null)
            return ValueTask.FromResult<IEnumerable<Claim>?>(null);

        var claimsInfo = serializer.Deserialize<ClaimsInfo>(json);
        return ValueTask.FromResult<IEnumerable<Claim>?>(CreateClaims(claimsInfo));
    }

    #region Private Methods
    private static List<Claim>? CreateClaims(ClaimsInfo? claimsInfo)
    {
        if (claimsInfo is null)
            return null;

        var claimTypes = OIUConfig.Instance.Claims;
        var claims = new List<Claim>
        {
            new (claimTypes.Subject, claimsInfo.Id.ToString("N")),
            new (claimTypes.FirstName, claimsInfo.FirstName?.ToString() ?? string.Empty),
            new (claimTypes.LastName, claimsInfo.LastName?.ToString() ?? string.Empty),

            new (ClaimTypes.Name, claimsInfo.Username?.ToString() ?? string.Empty),
            new (claimTypes.PreferredUsername, claimsInfo.Username?.ToString() ?? string.Empty),

            new (claimTypes.ClientId, claimsInfo.ClientId?.ToString() ?? string.Empty),
            new (claimTypes.Email, claimsInfo.Email?.ToString() ?? string.Empty),
            new (claimTypes.EmailVerified, claimsInfo.EmailVerified.ToString(), ClaimValueTypes.Boolean),
            new (claimTypes.Phone, claimsInfo.Phone?.ToString() ?? string.Empty),
            new (claimTypes.PhoneVerified, claimsInfo.PhoneVerified.ToString(), ClaimValueTypes.Boolean),
            new (claimTypes.Name, $"{claimsInfo.FirstName} {claimsInfo.LastName}")
        };

        // Set scope and role in the same claims
        if (claimsInfo.Role is not null)
            foreach (var role in claimsInfo.Role.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
                claims.Add(new Claim(claimTypes.Role, role));
            }
        if (claimsInfo.Scope is not null)
            foreach (var scope in claimsInfo.Scope.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()))
                claims.Add(new Claim(claimTypes.Scope, scope));

        return claims;
    }
    #endregion

    #region Nested Classes
    /// <summary>
    /// User structure for all user in system
    /// </summary>
    private sealed class ClaimsInfo
    {
        /// <summary>
        /// User identifier, match with the subject
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// User name 
        /// </summary>
        public string? Username { get; set; }
        /// <summary>
        /// First name of the user
        /// </summary>
        public string? FirstName { get; set; }
        /// <summary>
        /// Last name of the user.
        /// </summary>
        public string? LastName { get; set; }
        /// <summary>
        /// Identity server client used to authenticate the user.
        /// </summary>
        public string? ClientId { get; set; }
        /// <summary>
        /// User email address.
        /// </summary>
        public string? Email { get; set; }
        /// <summary>
        /// Indicate if the user has email confirmed
        /// </summary>
        public bool EmailVerified { get; set; }
        /// <summary>
        /// Phone number.
        /// </summary>
        public string? Phone { get; set; }
        /// <summary>
        /// Indicate if the user has phone confirmed
        /// </summary>
        public bool PhoneVerified { get; set; }
        /// <summary>
        /// Indicate if the profile is active.
        /// </summary>
        public bool IsProfileActive { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? Role { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? Scope { get; set; }
    }
    #endregion
}
