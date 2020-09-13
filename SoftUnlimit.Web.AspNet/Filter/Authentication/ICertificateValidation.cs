using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SoftUnlimit.Web.AspNet.Filter.Authentication
{
    /// <summary>
    /// 
    /// </summary>
    public interface ICertificateValidation
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="certificate"></param>
        /// <returns></returns>
        bool Validate(X509Certificate2 certificate);
    }
}
