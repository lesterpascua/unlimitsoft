using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoftUnlimit.Web.AspNet.Filter.Authorization
{
    /// <summary>
    /// Requiere x-api-key secret
    /// </summary>
    public class XApiKeyAuthorizationRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// Header where the value its arrive
        /// </summary>
        public const string Header = "X-API-KEY";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="apiKey"></param>
        public XApiKeyAuthorizationRequirement(string apiKey)
        {
            this.ApiKey = apiKey;
        }

        /// <summary>
        /// Api secret key.
        /// </summary>
        public string ApiKey { get; }
    }
}
