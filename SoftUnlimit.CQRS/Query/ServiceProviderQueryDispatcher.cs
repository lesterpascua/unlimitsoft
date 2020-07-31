using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SoftUnlimit.CQRS.Specification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Query
{
    /// <summary>
    /// Query provider dispatcher using and standard IServiceProvider to locate the QueryHandler associate with a query.
    /// </summary>
    public class ServiceProviderQueryDispatcher : CacheDispatcher, IQueryDispatcher
    {
        private readonly IServiceProvider _provider;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="useCache"></param>
        public ServiceProviderQueryDispatcher(IServiceProvider provider, bool useCache = true)
            : base(useCache)
        {
            this._provider = provider;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="args"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TEntity>> DispatchAsync<TEntity>(IQuery args, ISpecification<TEntity> filter)
            where TEntity : class
        {
            Type queryType = args.GetType();
            Type entityType = typeof(TEntity);

            var handler = GetQueryHandler(this._provider, entityType, queryType);
            IEnumerable<TEntity> collection = handler.IsAsync ? await ExecuteHandlerForQueryAsync<TEntity>(handler, args, queryType, this.UseCache) : ExecuteHandlerForQuery<TEntity>(handler, args, queryType, this.UseCache);

            IQueryable<TEntity> query = collection as IQueryable<TEntity>;
            if (filter != null)
            {
                if (filter.Criteria != null)
                {
                    if (query == null)
                    {
                        var function = filter.Criteria.Compile();
                        collection = collection.Where(function);
                    } else
                        query = query.Where(filter.Criteria);
                }

                if (query != null)
                {
                    if (filter.Includes != null && filter.Includes.Count() != 0)
                        query = query.ApplyInclude(filter.Includes);

                    if (filter is ISearchSpecification<TEntity> searchSpecification)
                    {
                        if (searchSpecification.Order != null)
                            query = query.ApplyOrdered(searchSpecification.Order);

                        if (searchSpecification.Pagging != null)
                        {
                            searchSpecification.Total = await query.CountAsync();
                            query = query.ApplyPagging(searchSpecification.Pagging.Page, searchSpecification.Pagging.PageSize);
                        }
                    }
                    return await query.ToArrayAsync();
                }
            }
            if (query != null)
                return await query.ToArrayAsync();
            return collection;
        }

        #region Private Methods

        private static IQueryHandler GetQueryHandler(IServiceProvider scopeProvider, Type entity, Type query)
        {
            Type serviceType = typeof(IQueryHandler<,>).MakeGenericType(entity, query);
            IQueryHandler queryHandler = (IQueryHandler)scopeProvider.GetService(serviceType);
            if (queryHandler == null)
                throw new KeyNotFoundException("There is no handler associated with this query");

            return queryHandler;
        }

        private static IQueryable<TEntity> ExecuteHandlerForQuery<TEntity>(IQueryHandler handler, IQuery args, Type queryType, bool useCache)
        {
            if (useCache)
            {
                var method = GetFromCache(queryType, handler, false);
                return (IQueryable<TEntity>)method.Invoke(handler, new object[] { args });
            }
            return ((dynamic)handler).Handler((dynamic)args);
        }
        private static Task<IEnumerable<TEntity>> ExecuteHandlerForQueryAsync<TEntity>(IQueryHandler handler, IQuery args, Type queryType, bool useCache)
        {
            if (useCache)
            {
                var method = GetFromCache(queryType, handler, true);
                return (Task<IEnumerable<TEntity>>)method.Invoke(handler, new object[] { args });
            }
            return ((dynamic)handler).HandlerAsync((dynamic)args);
        }

        #endregion
    }
}
