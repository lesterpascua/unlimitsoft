using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoftUnlimit.Web.AspNet.Filter.Authorization
{
    /// <summary>
    /// 
    /// </summary>
    public class XApiKeyAuthorizationHandler : AuthorizationHandler<XApiKeyAuthorizationRequirement>
    {
        private readonly IHttpContextAccessor _httpContext;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpContext"></param>
        public XApiKeyAuthorizationHandler(IHttpContextAccessor httpContext)
        {
            this._httpContext = httpContext;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="requirement"></param>
        /// <returns></returns>
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, XApiKeyAuthorizationRequirement requirement)
        {
            var headers = this._httpContext.HttpContext.Request.Headers;
            if (headers[XApiKeyAuthorizationRequirement.Header] != requirement.ApiKey)
                context.Fail();
            context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
