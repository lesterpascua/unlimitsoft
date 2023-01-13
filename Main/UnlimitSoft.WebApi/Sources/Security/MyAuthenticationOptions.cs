using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using UnlimitSoft.Json;
using UnlimitSoft.Web.AspNet.Security.Authentication;
using UnlimitSoft.Web.Security;

namespace UnlimitSoft.WebApi.Sources.Security;


public class MyAuthenticationOptions : ApiKeyAuthenticationOptions
{
    /// <summary>
    /// 
    /// </summary>
    public const string AuthorizationHeader = "Authorization";

    /// <inheritdoc />
    protected override ValueTask<IEnumerable<Claim>?> CreateClaims(IServiceProvider provider, HttpRequest httpRequest, string apiKey)
    {
        if (!httpRequest.Headers.TryGetValue(AuthorizationHeader, out StringValues value))
            return ValueTask.FromResult<IEnumerable<Claim>?>(null);

        var json = value.First();
        if (json.StartsWith("ApiKey "))
        {
            json = json
                .Split(' ', 2, StringSplitOptions.None)
                .Skip(1)
                .First();

            json = Encoding.UTF8.GetString(Convert.FromBase64String(json));
        }
        var identity = JsonUtil.Deserialize<IdentityInfo>(json);
        if (identity is null)
            return ValueTask.FromResult<IEnumerable<Claim>?>(null);

        var claims = new List<Claim> {
            new Claim(ClaimExtension.Subject, identity.Id.ToString("N")),
        };

        if (identity.Scope != null)
            foreach (var scope in identity.Scope.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()))
                claims.Add(new Claim(ClaimExtension.Scope, scope));

        if (identity.Role != null)
            foreach (var role in identity.Role.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
                claims.Add(new Claim(ClaimExtension.Role, role));
            }

        return ValueTask.FromResult<IEnumerable<Claim>?>(claims);
    }
}
