using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.CQRS.Event.Configuration
{
    /// <summary>
    /// Creadential to autenticate
    /// </summary>
    public class Credential
    {
        /// <summary>
        /// User name
        /// </summary>
        public string User { get; set; }
        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// Secret key to access.
        /// </summary>
        public string SecretKey { get; set; }
    }
}
