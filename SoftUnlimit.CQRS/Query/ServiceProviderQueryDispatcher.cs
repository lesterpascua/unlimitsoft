using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.DependencyInjection;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Message;
using SoftUnlimit.CQRS.Query.Compliance;
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
        private readonly bool _validate;
        private readonly string _invalidArgumendText;
        private readonly Func<IList<ValidationFailure>, IDictionary<string, IEnumerable<string>>> _errorTransforms;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="validate"></param>
        /// <param name="useCache"></param>
        /// <param name="errorTransforms"></param>
        /// <param name="invalidArgumendText"></param>
        public ServiceProviderQueryDispatcher(IServiceProvider provider, bool validate = true, bool useCache = true,
            Func<IList<ValidationFailure>, IDictionary<string, IEnumerable<string>>> errorTransforms = null, string invalidArgumendText = null)
            : base(useCache)
        {
            _provider = provider;
            _validate = validate;
            _errorTransforms = errorTransforms;
            _invalidArgumendText = invalidArgumendText;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="args"></param>
        /// <returns></returns>
        public async Task<QueryResponse> DispatchAsync<TResult>(IQuery args)
        {
            Type queryType = args.GetType();
            Type entityType = typeof(TResult);

            if (_validate)
            {
                var errors = await ValidateAsync(_provider, args, ServiceProviderCommandDispatcher.DefaultErrorTransforms);
                if (errors != null)
                    return args.BadResponse(errors, _invalidArgumendText);
            }
            //
            // before execute query search if has compliance and executed
            var response = await CheckAndExecuteQueryComplianceAsync(_provider, args);
            if (response?.IsSuccess == false)
                return response;

            var handler = GetQueryHandler(_provider, entityType, queryType);
            TResult result = await ExecuteHandlerForQueryAsync<TResult>(handler, args, queryType, UseCache);

            return args.OkResponse(result);
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
            Type commandType = args.GetType();
            Type commandComplianceType = typeof(IQueryCompliance<>).MakeGenericType(commandType);

            IQueryCompliance queryCompliance = (IQueryCompliance)provider.GetService(commandComplianceType);
            if (queryCompliance != null)
                return await queryCompliance.ExecuteAsync(args);
            return null;
        }

        /// <summary>
        /// Register CommandCompliance in DPI.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="queryComplianceInterface">Interface used to register tha binding between CommandCompliance and command.</param>
        /// <param name="complianceAssembly"></param>
        public static void RegisterQueryCompliance(IServiceCollection services, Type queryComplianceInterface, IEnumerable<Assembly> complianceAssembly)
        {
            List<Type> existCommandCompliance = new List<Type>();
            foreach (var assembly in complianceAssembly)
            {
                var query = assembly
                    .GetTypes()
                    .Where(p => p.IsClass && p.IsAbstract == false && p.GetInterfaces().Contains(typeof(IQueryCompliance)));
                existCommandCompliance.AddRange(query);
            }

            foreach (var commandComplianceImplementation in existCommandCompliance)
            {
                var commandComplianceImplementedInterfaces = commandComplianceImplementation.GetInterfaces()
                    .Where(p =>
                        p.IsGenericType &&
                        p.GetGenericArguments().Length == 1 &&
                        p.GetGenericTypeDefinition() == queryComplianceInterface);

                foreach (var complianceInterface in commandComplianceImplementedInterfaces)
                {
                    var commandType = complianceInterface.GetGenericArguments().Single();
                    var wellKnowCommandComplianceInterface = typeof(IQueryCompliance<>).MakeGenericType(commandType);

                    services.AddScoped(wellKnowCommandComplianceInterface, commandComplianceImplementation);
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
        private static Task<TEntity> ExecuteHandlerForQueryAsync<TEntity>(IQueryHandler handler, IQuery args, Type queryType, bool useCache)
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
