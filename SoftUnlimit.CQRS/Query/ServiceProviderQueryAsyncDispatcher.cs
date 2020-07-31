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
    public class ServiceProviderQueryAsyncDispatcher : CacheDispatcher, IQueryAsyncDispatcher
    {
        private readonly IServiceProvider _provider;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="useCache"></param>
        public ServiceProviderQueryAsyncDispatcher(IServiceProvider provider, bool useCache = true)
            : base(useCache)
        {
            this._provider = provider;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="args"></param>
        /// <returns></returns>
        public async Task<TResult> DispatchAsync<TResult>(IQueryAsync args)
            where TResult : class
        {
            Type queryType = args.GetType();
            Type entityType = typeof(TResult);

            var handler = GetQueryHandler(this._provider, entityType, queryType);
            TResult result = await ExecuteHandlerForQueryAsync<TResult>(handler, args, queryType, this.UseCache);

            return result;
        }

        #region Private Methods

        private static IQueryAsyncHandler GetQueryHandler(IServiceProvider scopeProvider, Type entity, Type query)
        {
            Type serviceType = typeof(IQueryAsyncHandler<,>).MakeGenericType(entity, query);
            IQueryAsyncHandler queryHandler = (IQueryAsyncHandler)scopeProvider.GetService(serviceType);
            if (queryHandler == null)
                throw new KeyNotFoundException("There is no handler associated with this query");

            return queryHandler;
        }

        private static Task<TEntity> ExecuteHandlerForQueryAsync<TEntity>(IQueryAsyncHandler handler, IQueryAsync args, Type queryType, bool useCache)
        {
            if (useCache)
            {
                var method = GetFromCache(queryType, handler, true);
                return (Task<TEntity>)method.Invoke(handler, new object[] { args });
            }
            return ((dynamic)handler).HandlerAsync((dynamic)args);
        }


        #endregion
    }
}
