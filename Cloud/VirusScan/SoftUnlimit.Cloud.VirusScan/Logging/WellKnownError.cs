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
        BadRequest_Error = 4 * 1000,
        /// <summary>
        /// 
        /// </summary>
        BadRequest_RequestMustExist = 4 * 1001,
        /// <summary>
        /// 
        /// </summary>
        BadRequest_RequestInvalidStatus = 4 * 1002,
    }
}
