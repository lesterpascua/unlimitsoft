using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SoftUnlimit.Web.AspNet.Filter.Authentication
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class DefaultCertificateValidation : ICertificateValidation
    {
        private readonly X509Certificate2 _certificate;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="certificate"></param>
        /// <param name="password"></param>
        public DefaultCertificateValidation(string certificate, string password)
        {
            _certificate = Load(certificate, password);
        }
        /// <inheritdoc />
        public bool Validate(X509Certificate2 certificate) => _certificate.Thumbprint == certificate.Thumbprint;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rawCertificate"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static X509Certificate2 Load(string rawCertificate, string password)
        {
            if (string.IsNullOrWhiteSpace(rawCertificate))
                return null;
            try
            {
                string cleanHeaderValue = Uri.UnescapeDataString(rawCertificate);
                byte[] bytes = Convert.FromBase64String(cleanHeaderValue);
                return new X509Certificate2(bytes, password);
            } catch { }

            return null;
        }
    }
}
