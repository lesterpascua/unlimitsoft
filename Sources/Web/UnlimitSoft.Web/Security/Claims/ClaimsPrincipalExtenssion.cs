using System.Linq;
using System.Security.Claims;

namespace UnlimitSoft.Web.Security.Claims;


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
        var claim = claims.FindAll(m => m.Type == "sub").FirstOrDefault();
        if (claim is not null)
            return claim.Value;
        return string.Empty;
    }
}
