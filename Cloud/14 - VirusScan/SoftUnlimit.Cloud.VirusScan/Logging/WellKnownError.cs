using System.ComponentModel;

namespace SoftUnlimit.Cloud.VirusScan.Logging
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
        /// <summary>
        /// 
        /// </summary>
        BadRequest_RequestMustExist = Resources.ServiceId * 1001,
        /// <summary>
        /// 
        /// </summary>
        BadRequest_RequestInvalidStatus = Resources.ServiceId * 1002,
    }
}
