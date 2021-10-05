using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;

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
        public string Scheme => DefaultAuthenticationScheme;
        /// <summary>
        /// Authentication type.
        /// </summary>
        public string AuthenticationType => DefaultAuthenticationScheme;

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
        /// <param name="httpRequest"></param>
        /// <returns></returns>
        protected internal abstract IEnumerable<Claim> CreateClaims(HttpRequest httpRequest);
    }
}
