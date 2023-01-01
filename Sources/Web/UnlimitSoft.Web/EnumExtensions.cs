using System;
using System.Linq;
using System.Reflection;

namespace UnlimitSoft.Web;


/// <summary>
/// 
/// </summary>
public static class EnumExtenssion
{
    /// <summary>
    /// If exist PretyNameAttribute return this if not return standard to string.
    /// </summary>
    /// <param name="enum"></param>
    /// <returns></returns>
    [Obsolete("use specifx attibute")]
    public static string ToPrettyString(this Enum @enum)
    {
        string value = @enum.ToString();
        MemberInfo[] memberInfo = @enum.GetType().GetMember(value);
        if (memberInfo?.Any() == true)
        {
            object[] attrs = memberInfo[0].GetCustomAttributes(typeof(PrettyNameAttribute), false);
            if (attrs?.Any() == true)
                value = ((PrettyNameAttribute)attrs[0]).Name;
        }

        return value;
    }
}
