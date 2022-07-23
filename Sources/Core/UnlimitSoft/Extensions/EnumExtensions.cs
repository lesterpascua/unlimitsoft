using System.Text;

namespace UnlimitSoft.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="values"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string ToStringSeparator(this int[] values, char separator = ',')
        {
            var sb = new StringBuilder();
            foreach (var v in values)
                sb.Append(v).Append(separator);
            return sb.ToString(0, sb.Length - 1);
        }
    }
}