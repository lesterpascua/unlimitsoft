using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.Web.AspNet.Health
{
    /// <summary>
    /// Health middleware configuration settings.
    /// </summary>
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
        /// <summary>
        /// 
        /// </summary>
        public string[] SkipUrls { get; set; } = new string[] { "/hc" };
    }
}
