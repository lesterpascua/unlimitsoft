using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.Web.Configuration
{
    /// <summary>
    /// Se usa para establecer los parametros que necesita el proyecto para conectarse al identity server.
    /// </summary>
    public class IdentityServerCredential
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Ssl { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ApiName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ApiSecret { get; set; }
    }
}
