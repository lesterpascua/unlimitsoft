using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.Web.Configuration
{
    /// <summary>
    /// 
    /// </summary>
    public class IdentityServerSettings
    {
        /// <summary>
        /// End point con la autoridad de identity server.
        /// </summary>
        public string Authority { get; set; }

        /// <summary>
        /// Configuracion de las credenciales del proyecto para conectarse al identity server.
        /// </summary>
        public IdentityServerCredential SystemScope { get; set; }
    }
}
