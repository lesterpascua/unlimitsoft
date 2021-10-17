using System.ComponentModel;

namespace SoftUnlimit.Cloud.Partner.Logging
{
    /// <summary>
    /// 
    /// </summary>
    public enum WellKnownError
    {
        /// <summary>
        /// Generic service error
        /// </summary>
        [Description("Generic service error")]
        BadRequest_Error = Resources.ServiceId * 1000,
    }
}
