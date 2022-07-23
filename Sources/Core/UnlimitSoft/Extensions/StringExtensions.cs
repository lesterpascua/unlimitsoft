using System;

namespace UnlimitSoft.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Asume the string is a separator , int representation
        /// </summary>
        /// <param name="this"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static int[] ToIntArray(this string @this, char separator = ',')
        {
            if (@this is null)
                return Array.Empty<int>();

            var parts = @this.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);
            var array = new int[parts.Length];
            for (var i = 0; i < parts.Length; i++)
                array[i] = int.Parse(parts[i]);

            return array;
        }
    }
}