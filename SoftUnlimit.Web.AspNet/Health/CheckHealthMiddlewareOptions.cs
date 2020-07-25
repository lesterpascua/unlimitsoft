using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.Web.AspNet.Health
{
    public class CheckHealthMiddlewareOptions
    {

        /// <summary>
        /// 
        /// </summary>
        public uint RetryAfter { get; set; } = 30;
        /// <summary>
        /// 
        /// </summary>
        public string InfoText { get; set; } = "Service Unavailable";
    }
}
