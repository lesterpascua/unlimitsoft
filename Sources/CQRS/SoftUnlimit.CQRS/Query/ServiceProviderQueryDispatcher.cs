using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using SoftUnlimit.CQRS.Cache;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Message;
using SoftUnlimit.CQRS.Query.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Query
{
    /// <summary>
    /// Query provider dispatcher using and standard IServiceProvider to locate the QueryHandler associate with a query.
    /// </summary>
    public class ServiceProviderQueryDispatcher : CacheDispatcher, IQueryDispatcher
    {
        private readonly IServiceProvider _provider;
        private readonly bool _validate;
        private readonly string _invalidArgumendText;
        private readonly ILogger<ServiceProviderQueryDispatcher> _logger;
        private readonly Func<IList<ValidationFailure>, IDictionary<string, IEnumerable<string>>> _errorTransforms;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="validate"></param>
        /// <param name="useCache"></param>
        /// <param name="errorTransforms"></param>
        /// <param name="invalidArgumendText"></param>
        /// <param name="logger"></param>
        public ServiceProviderQueryDispatcher(IServiceProvider provider, bool validate = true, bool useCache = true,
            Func<IList<ValidationFailure>, IDictionary<string, IEnumerable<string>>> errorTransforms = null, string invalidArgumendText = null, 
            ILogger<ServiceProviderQueryDispatcher> logger = null
        )
            : base(useCache)
        {
            _provider = provider;
            _validate = validate;
            _errorTransforms = errorTransforms ?? ServiceProviderCommandDispatcher.DefaultErrorTransforms;
            _invalidArgumendText = invalidArgumendText;
            _logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="query"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<IQueryResponse> DispatchAsync<TResult>(IQuery query, CancellationToken ct = default)
        {
            var queryType = query.GetType();
            var entityType = typeof(TResult);

            _logger?.LogDebug("Execute Query type: {Type}", queryType);

            var handler = GetQueryHandler(_provider, entityType, queryType);
            var handlerType = handler.GetType();

            dynamic dynamicHandler = handler;
            dynamic dynamicQuery = query;
            if (_validate)
            {
                var interfaces = handlerType.GetInterfaces();

                #region Verify if query implement internal validation
                var validationHandlerType = typeof(IQueryHandlerValidator<>).MakeGenericType(queryType);
                if (interfaces.Any(type => type == validationHandlerType))
                {
                    _logger?.LogDebug("Query handler implement internal validation");

                    var validatorType = typeof(QueryValidator<>).MakeGenericType(queryType);
                    IValidator validator = (IValidator)Activator.CreateInstance(validatorType);

                    validator = await (ValueTask<IValidator>)dynamicHandler.ValidatorAsync(dynamicQuery, (dynamic)validator, ct);

                    var valContext = new ValidationContext<IQuery>(query);
                    var errors = await validator.ValidateAsync(valContext, ct);

                    _logger?.LogDebug("Evaluate validator process result: {@Errors}", errors);
                    if (errors?.IsValid == false)
                    {
                        if (_errorTransforms == null)
                            return query.BadResponse(errors.Errors, _invalidArgumendText);
                        return query.BadResponse(_errorTransforms(errors.Errors), _invalidArgumendText);
                    }
                }
                else
                    _logger?.LogDebug("Query not handler implement internal validation");
                #endregion

                #region Verify if Query implement internal compliance
                var complianceHandlerType = typeof(IQueryHandlerCompliance<>).MakeGenericType(queryType);
                if (interfaces.Any(type => type == complianceHandlerType))
                {
                    _logger?.LogDebug("Query handler implement internal compliance");

                    var response = await (Task<IQueryResponse>)dynamicHandler.HandleComplianceAsync(dynamicQuery, ct);
                    if (!response.IsSuccess)
                        return response;
                }
                else
                    _logger?.LogDebug("Query not handler implement internal compliance");
                #endregion
            }
            TResult result = await ExecuteHandlerForQueryAsync<TResult>(handler, dynamicHandler, query, dynamicQuery, queryType, UseCache, ct);

            return query.OkResponse(result);
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
        private static async Task<TEntity> ExecuteHandlerForQueryAsync<TEntity>(IQueryHandler handler, dynamic dynamicHandler, IQuery query, dynamic dynamicQuery, Type queryType, bool useCache, CancellationToken ct)
        {
            Task<TEntity> result;
            if (useCache)
            {
                var method = GetQueryHandlerFromCache<TEntity>(queryType, handler);
                result = (Task<TEntity>)method(handler, query, ct);
            } else
                result = (Task<TEntity>)dynamicHandler.HandleAsync(dynamicQuery, ct);

            return await result;
        }
        
        #endregion
    }
}
