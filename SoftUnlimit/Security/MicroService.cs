using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.Security
{
    /// <summary>
    /// Micro service information.
    /// </summary>
    public class MicroService
    {
        private MicroService(uint serviceId, string workerId) => (ServiceId, WorkerId) = (serviceId, workerId);

        /// <summary>
        /// 
        /// </summary>
        public uint ServiceId { get; }
        /// <summary>
        /// 
        /// </summary>
        public string WorkerId { get; set; }

        /// <summary>
        /// Get current service configuration.
        /// </summary>
        public static MicroService Default { get; private set; }
        /// <summary>
        /// Init service information
        /// </summary>
        /// <param name="serviceId"></param>
        /// <param name="workerId"></param>
        /// <returns></returns>
        public static MicroService Init(uint serviceId, string workerId)
        {
            //if (Default != null)
            //    throw new InvalidOperationException("Only one initialization allowed");
            return Default = new MicroService(serviceId, workerId);
        }
    }
}
