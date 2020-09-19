using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using SoftUnlimit.Web.Security;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace SoftUnlimit.Web.AspNet.Filter.Authentication
{
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
        /// 
        /// </summary>
        public Func<HttpRequest, TUser> CreateUserInfo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Func<TUser, HttpRequest, IEnumerable<Claim>> CreateClaims { get; set; }
    }
}
