using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SoftUnlimit.Web.AspNet.Security.Authentication
{
    /// <summary>
    /// Options for API Key auth method
    /// </summary>
    public abstract class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
    {
        /// <summary>
        /// 
        /// </summary>
        public const string DefaultAuthenticationScheme = "ApiKey";

        /// <summary>
        /// Sheme name
        /// </summary>
        public virtual string Scheme => DefaultAuthenticationScheme;
        /// <summary>
        /// Authentication type.
        /// </summary>
        public virtual string AuthenticationType => DefaultAuthenticationScheme;

        /// <summary>
        /// Api key used to compare.
        /// </summary>
        public string ApiKey { get; set; }
        /// <summary>
        /// Supplied error code and get string representation.
        /// </summary>
        protected internal Func<ApiKeyError, string> ErrorBuilder { get; }

        /// <summary>
        /// Return claims associate to principal identity. The result could be null. 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="httpRequest"></param>
        /// <param name="apiKey"></param>
        /// <returns></returns>
        protected internal abstract ValueTask<IEnumerable<Claim>> CreateClaims(IServiceProvider provider, HttpRequest httpRequest, string apiKey);
    }
}
