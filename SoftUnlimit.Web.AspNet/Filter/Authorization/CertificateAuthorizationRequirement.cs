using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace SoftUnlimit.Web.AspNet.Filter.Authorization
{
    /// <summary>
    /// Require certificate validations
    /// </summary>
    public class CertificateAuthorizationRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="certificate"></param>
        public CertificateAuthorizationRequirement(X509Certificate2 certificate)
        {
            this.Certificate = certificate;
        }

        /// <summary>
        /// 
        /// </summary>
        public X509Certificate2 Certificate { get; }
    }
}
