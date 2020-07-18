using AutoMapper;
using SoftUnlimit.CQRS.Specification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Query
{
    /// <summary>
    /// 
    /// </summary>
    public interface IQueryDispatcher
    {
        /// <summary>
        /// Dispatch query associate with specified handler.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="query"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<IEnumerable<TEntity>> DispatchAsync<TEntity>(IQuery query, ISpecification<TEntity> filter = null) where TEntity : class;
    }
    /// <summary>
    /// Extenssion method for query dispatcher.
    /// </summary>
    public static class IQueryDispatcherExtenssion
    {
        /// <summary>
        /// Excecute result DispatchAsync and convert result to a TDto specified.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="self"></param>
        /// <param name="mapper"></param>
        /// <param name="query"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static Task<IEnumerable<TResult>> DispatchAsync<TEntity, TResult>(this IQueryDispatcher self, IMapper mapper, IQuery query, ISpecification<TEntity> filter = null)
            where TEntity : class
        {
            return self.DispatchAsync(query, filter).ContinueWith(c => c.Result.Select(s => mapper.Map<TResult>(s)));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="dispatcher"></param>
        /// <param name="query"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static Task<TEntity> DispatchSingleAsync<TEntity>(this IQueryDispatcher dispatcher, IQuery query, ISpecification<TEntity> filter = null)
            where TEntity : class
        {
            return dispatcher.DispatchAsync(query, filter).ContinueWith(c => c.Result.SingleOrDefault());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="dispatcher"></param>
        /// <param name="mapper"></param>
        /// <param name="query"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static Task<TResult> DispatchSingleAsync<TEntity, TResult>(this IQueryDispatcher dispatcher, IMapper mapper, IQuery query, ISpecification<TEntity> filter = null)
            where TEntity : class
        {
            return dispatcher.DispatchSingleAsync(query, filter).ContinueWith(c => mapper.Map<TResult>(c.Result));
        }
    }
}
