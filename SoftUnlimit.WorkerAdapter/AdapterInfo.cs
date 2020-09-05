using Microsoft.Extensions.HealthChecks;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.WorkerAdapter
{
    /// <summary>
    /// Information about adapter.
    /// </summary>
    public class AdapterInfo
    {
        /// <summary>
        /// Service identifier
        /// </summary>
        public uint ServiceID { get; set; }
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

        /// <summary>
        /// Create adapter info from storage object and service.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="checkService"></param>
        /// <returns></returns>
        public static AdapterInfo FromAdapterInfoStorageObject(IAdapterInfoStorageObject data, IHealthCheckService checkService) => new AdapterInfo {
            Checker = checkService,
            Created = data.Created,
            Endpoint = data.Endpoint,
            Identifier = data.Identifier,
            ServiceID = data.ServiceID,
            Updated = data.Updated,
            WorkerID = data.WorkerID
        };
    }
}
