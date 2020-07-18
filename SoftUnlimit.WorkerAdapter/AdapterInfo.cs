using Microsoft.Extensions.HealthChecks;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.WorkerAdapter
{
    /// <summary>
    /// 
    /// </summary>
    public class AdapterInfo
    {
        /// <summary>
        /// Service identifier
        /// </summary>
        public int Service { get; set; }
        /// <summary>
        /// Worker numeric identifier
        /// </summary>
        public ushort WorkerID { get; set; }
        /// <summary>
        /// Worker string identifier
        /// </summary>
        public string Identifier { get; set; }
        /// <summary>
        /// Service encpoint
        /// </summary>
        public string Endpoint { get; set; }
        /// <summary>
        /// Checker to get service healthly
        /// </summary>
        public IHealthCheckService Checker { get; set; }
        /// <summary>
        /// Creation date
        /// </summary>
        public DateTime Created { get; set; }
        /// <summary>
        /// Last good healthly check
        /// </summary>
        public DateTime Updated { get; set; }
    }
}
