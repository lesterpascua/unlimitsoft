using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SoftUnlimit.Web
{
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
        public static string ToPretyString(this Enum @enum)
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
}
