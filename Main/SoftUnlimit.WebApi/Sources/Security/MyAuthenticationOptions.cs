using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using SoftUnlimit.Json;
using SoftUnlimit.Web.AspNet.Security.Authentication;
using SoftUnlimit.Web.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace SoftUnlimit.WebApi.Sources.Security
{
    public class MyAuthenticationOptions : ApiKeyAuthenticationOptions
    {
        /// <summary>
        /// 
        /// </summary>
        public const string AuthorizationHeader = "Authorization";


        protected override IEnumerable<Claim> CreateClaims(HttpRequest httpRequest)
        {
            if (!httpRequest.Headers.TryGetValue(AuthorizationHeader, out StringValues value))
                return null;

            var json = value.FirstOrDefault();
            if (json.StartsWith("ApiKey "))
            {
                json = json
                    .Split(' ', 2, StringSplitOptions.None)
                    .Skip(1)
                    .FirstOrDefault();

                json = Encoding.UTF8.GetString(Convert.FromBase64String(json));
            }
            var identity = JsonUtility.Deserializer<IdentityInfo>(json);

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

            return claims;
        }
    }
}
