namespace SoftUnlimit.Data
{
    /// <summary>
    /// 
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// Entity identifier.
        /// </summary>
        object Id { get; }

        /// <summary>
        /// Indicate is not initialized yet.
        /// </summary>
        /// <returns></returns>
        bool IsTransient();
    }
}
