using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Message;
using SoftUnlimit.CQRS.Query.Compliance;
using SoftUnlimit.CQRS.Query.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        public async Task<QueryResponse> DispatchAsync<TResult>(IQuery query, CancellationToken ct = default)
        {
            Type queryType = query.GetType();
            Type entityType = typeof(TResult);

            if (_validate)
            {
                var errors = await ValidateAsync(_provider, query, _errorTransforms);
                if (errors != null)
                    return query.BadResponse(errors, _invalidArgumendText);
            }
            //
            // before execute query search if has compliance and executed
            var response = await CheckAndExecuteQueryComplianceAsync(_provider, query);
            if (response?.IsSuccess == false)
                return response;

            _logger?.LogDebug("Execute Query type: {Type}", queryType);
            var handler = GetQueryHandler(_provider, entityType, queryType);

            #region Verify if query implement internal validation
            if (handler is IQueryHandlerValidator QueryHandlerValidator)
            {
                _logger?.LogDebug("Query handler implement internal validation");

                var validatorType = typeof(QueryValidator<>).MakeGenericType(queryType);
                IValidator validator = (IValidator)Activator.CreateInstance(validatorType);
                validator = QueryHandlerValidator.BuildValidator(validator);

                var valContext = new ValidationContext<IQuery>(query);
                var errors = await validator.ValidateAsync(valContext);

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
            if (handler is IQueryHandlerCompliance queryHandlerCompliance)
            {
                _logger?.LogDebug("Query handler implement internal compliance");

                response = await queryHandlerCompliance.HandleComplianceAsync(query);
                if (!response.IsSuccess)
                    return response;
            }
            else
                _logger?.LogDebug("Query not handler implement internal compliance");
            #endregion

            TResult result = await ExecuteHandlerForQueryAsync<TResult>(handler, query, queryType, UseCache, ct);

            return query.OkResponse(result);
        }

        /// <summary>
        /// Validate a query.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="query"></param>
        /// <param name="errorTransforms"></param>
        /// <returns></returns>
        public static async Task<object> ValidateAsync(IServiceProvider provider, IQuery query, Func<IList<ValidationFailure>, IDictionary<string, IEnumerable<string>>> errorTransforms = null)
        {
            var queryType = query.GetType();
            var validatorType = typeof(IValidator<>).MakeGenericType(queryType);
            if (provider.GetService(validatorType) is IValidator validator)
            {
                var valContext = new ValidationContext<IQuery>(query);
                var errors = await validator.ValidateAsync(valContext);
                if (errors?.IsValid == false)
                {
                    if (errorTransforms == null)
                        return errors.Errors;
                    return errorTransforms(errors.Errors);
                }
            }
            return null;
        }
        /// <summary>
        /// Check if exist compliance asociate to this query and executed.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static async Task<QueryResponse> CheckAndExecuteQueryComplianceAsync(IServiceProvider provider, IQuery args)
        {
            Type QueryType = args.GetType();
            Type QueryComplianceType = typeof(IQueryCompliance<>).MakeGenericType(QueryType);

            IQueryCompliance queryCompliance = (IQueryCompliance)provider.GetService(QueryComplianceType);
            if (queryCompliance != null)
                return await queryCompliance.ExecuteAsync(args);
            return null;
        }

        /// <summary>
        /// Register QueryCompliance in DPI.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="queryComplianceInterface">Interface used to register tha binding between QueryCompliance and Query.</param>
        /// <param name="complianceAssembly"></param>
        public static void RegisterQueryCompliance(IServiceCollection services, Type queryComplianceInterface, IEnumerable<Assembly> complianceAssembly)
        {
            List<Type> existQueryCompliance = new List<Type>();
            foreach (var assembly in complianceAssembly)
            {
                var query = assembly
                    .GetTypes()
                    .Where(p => p.IsClass && p.IsAbstract == false && p.GetInterfaces().Contains(typeof(IQueryCompliance)));
                existQueryCompliance.AddRange(query);
            }

            foreach (var QueryComplianceImplementation in existQueryCompliance)
            {
                var QueryComplianceImplementedInterfaces = QueryComplianceImplementation.GetInterfaces()
                    .Where(p =>
                        p.IsGenericType &&
                        p.GetGenericArguments().Length == 1 &&
                        p.GetGenericTypeDefinition() == queryComplianceInterface);

                foreach (var complianceInterface in QueryComplianceImplementedInterfaces)
                {
                    var QueryType = complianceInterface.GetGenericArguments().Single();
                    var wellKnowQueryComplianceInterface = typeof(IQueryCompliance<>).MakeGenericType(QueryType);

                    services.AddScoped(wellKnowQueryComplianceInterface, QueryComplianceImplementation);
                }
            }
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
        private static async Task<TEntity> ExecuteHandlerForQueryAsync<TEntity>(IQueryHandler handler, IQuery args, Type queryType, bool useCache, CancellationToken ct)
        {
            if (useCache)
            {
                var method = GetFromCache(queryType, handler);
                return await (Task<TEntity>)method.Invoke(handler, new object[] { args, ct });
            }
            return await ((dynamic)handler).HandlerAsync((dynamic)args, ct);
        }
        
        #endregion
    }
}
