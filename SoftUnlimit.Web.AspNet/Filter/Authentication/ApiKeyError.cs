using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.Web.AspNet.Filter.Authentication
{
    public enum ApiKeyError
    {
        InvalidAPIKey = 1,
        InvalidUserInfo = 2,
        InvalidUserPermission = 3
    }
}
