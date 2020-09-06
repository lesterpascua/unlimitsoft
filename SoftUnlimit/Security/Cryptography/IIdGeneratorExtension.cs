using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace SoftUnlimit.Security.Cryptography
{
    /// <summary>
    /// 
    /// </summary>
    public static class IIdGeneratorExtension
    {
        /// <summary>
        /// Gemerate new id and convert to string.
        /// </summary>
        /// <param name="gen"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GenerateIdAsString(this IIdGenerator<Guid> gen) => gen.GenerateId().ToString();
        /// <summary>
        /// Gemerate new id and convert to string.
        /// </summary>
        /// <param name="gen"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GenerateIdAsString(this IIdGenerator<long> gen) => gen.GenerateId().ToString();
    }
}
