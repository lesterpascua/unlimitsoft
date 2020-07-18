using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace SoftUnlimit.Web.Security.Claims
{
    /// <summary>
    /// 
    /// </summary>
    public static class ClaimsPrincipalExtenssion
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="claims"></param>
        /// <returns></returns>
        public static string GetSubjectId(this ClaimsPrincipal claims)
        {
            Claim claim = claims.FindAll(m => m.Type == "sub").SingleOrDefault();
            if (claim != null)
                return claim.Value;
            return string.Empty;
        }
    }
}
