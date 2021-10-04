using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using SoftUnlimit.Web.Security;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace SoftUnlimit.Web.AspNet.Filter.Authentication
{
    /// <summary>
    /// Options for API Key auth method
    /// </summary>
    /// <typeparam name="TUser"></typeparam>
    public class ApiKeyAuthenticationOptions<TUser> : AuthenticationSchemeOptions
        where TUser : class
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
        /// Retrive user associate with the request if null means no user asociate and the apiKey is invalid.
        /// </summary>
        public Func<HttpRequest, TUser> CreateUserInfo { get; set; }
        /// <summary>
        /// Return claims associate to principal identity. The result could be null. 
        /// </summary>
        public Func<TUser, HttpRequest, IEnumerable<Claim>> CreateClaims { get; set; }
    }
}
