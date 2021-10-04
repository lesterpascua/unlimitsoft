using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace SoftUnlimit.Web.AspNet.Filter.Authorization
{
    /// <summary>
    /// 
    /// </summary>
    public class CertificateAuthorizationHandler : AuthorizationHandler<CertificateAuthorizationRequirement>
    {
        private readonly IHttpContextAccessor _httpContext;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpContext"></param>
        public CertificateAuthorizationHandler(IHttpContextAccessor httpContext)
        {
            this._httpContext = httpContext;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="requirement"></param>
        /// <returns></returns>
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CertificateAuthorizationRequirement requirement)
        {
            var cert = requirement.Certificate;
            if (this._httpContext.HttpContext.Connection.ClientCertificate?.Thumbprint != cert.Thumbprint)
                context.Fail();
            context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
