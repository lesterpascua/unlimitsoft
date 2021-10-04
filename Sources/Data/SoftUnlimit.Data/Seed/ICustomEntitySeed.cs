using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.Data.Seed
{
    /// <summary>
    /// 
    /// </summary>
    public interface ICustomEntitySeed
    {
        /// <summary>
        /// Allow stablish how enumerator is executed first.
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Execute seed for the curr entity.
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task SeedAsync(CancellationToken ct = default);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface ICustomEntitySeed<TEntity> : ICustomEntitySeed
        where TEntity : class
    {
    }
    /// <summary>
    /// 
    /// </summary>
    public static class ICustomEntitySeedExtenssion
    {
        /// <summary>
        /// Get entity name.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="_"></param>
        /// <returns></returns>
        public static string GetName<TEntity>(this ICustomEntitySeed<TEntity> _) where TEntity : class => typeof(TEntity).Name;
    }
}
