namespace SoftUnlimit.Web.Client
{
    /// <summary>
    /// 
    /// </summary>
    public static class IResponseExtensions
    {
        /// <summary>
        /// Get body cast to specific type.
        /// </summary>
        /// <typeparam name="TBody"></typeparam>
        /// <returns></returns>
        public static TBody GetBody<TBody>(this IResponse self) => (TBody)self.GetBody();
    }
}
