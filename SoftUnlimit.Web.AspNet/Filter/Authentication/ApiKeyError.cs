using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.Web.AspNet.Filter.Authentication
{
    /// <summary>
    /// API key error codes.
    /// </summary>
    public enum ApiKeyError
    {
        /// <summary>
        /// Invalid key 
        /// </summary>
        InvalidAPIKey = 1,
        /// <summary>
        /// Invalid user data
        /// </summary>
        InvalidUserInfo = 2,
        /// <summary>
        /// User don't have pemission.
        /// </summary>
        InvalidUserPermission = 3
    }
}
