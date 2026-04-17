namespace OIU.Libs;


/// <summary>
/// 
/// </summary>
public static class UAssignExtensions
{
    extension(U.UAssign _)
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        /// <param name="action"></param>
        /// <param name="updated"></param>
        /// <returns></returns>
        public static bool Safe<T>(T src, T dest, Action<T> action, ref bool updated)
        {
            if (Equals(src, dest))
                return false;

            action(src);
            return updated = true;
        }
    }
}