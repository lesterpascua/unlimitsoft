using System;
using System.Collections.Generic;
using System.Text;
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
        /// 
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        Task SeedAsync(IUnitOfWork unitOfWork);
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
