using System.Runtime.CompilerServices;

namespace SoftUnlimit.Security.Cryptography
{
    /// <summary>
    /// 
    /// </summary>
    public static class IIdGeneratorExtensions
    {
        /// <summary>
        /// Gemerate new id and convert to string.
        /// </summary>
        /// <param name="gen"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GenerateIdAsString<T>(this IIdGenerator<T> gen) => gen.GenerateId().ToString();
    }
}
