using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using SoftUnlimit.CQRS.Command.Compliance;
using SoftUnlimit.CQRS.Command.Validation;
using SoftUnlimit.CQRS.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Command
{
    /// <summary>
    /// Implement a command dispatcher resolving all handler by a ServiceProvider.
    /// </summary>
    public class ServiceProviderCommandDispatcher : ICommandDispatcher
    {
        private readonly IServiceProvider _provider;
        private readonly bool _validate, _useCache, _useScope;
        private readonly Func<IList<ValidationFailure>, IDictionary<string, IEnumerable<string>>> _errorTransforms;
        private readonly string _invalidArgumendText;

        private static readonly object _sync = new object();
        private static readonly Dictionary<Type, MethodInfo> _cache = new Dictionary<Type, MethodInfo>();

        /// <summary>
        /// Default function used to convert error transform to standard ASP.NET format
        /// </summary>
        public static readonly Func<IList<ValidationFailure>, IDictionary<string, IEnumerable<string>>> DefaultErrorTransforms = (failure) => failure.GroupBy(p => p.PropertyName).ToDictionary(k => k.Key, v => v.Select(s => s.ErrorMessage))


        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="validate">Enable command validation after execute associate handler.</param>
        /// <param name="useCache">Store reference method of command handler associate by command to better performance.</param>
        /// <param name="useScope">Create scope to resolve element in DPI.</param>
        /// <param name="errorTransforms">Conver error to a Dictionary where key is a propertyName with an error and value is all error description.</param>
        /// <param name="invalidArgumendText">Default text used to response in Inotify object when validation not success.</param>
        public ServiceProviderCommandDispatcher(IServiceProvider provider, bool validate = true, bool useCache = true, bool useScope = true,
            Func<IList<ValidationFailure>, IDictionary<string, IEnumerable<string>>> errorTransforms = null, string invalidArgumendText = null)
        {
            this._provider = provider;
            this._validate = validate;
            this._useCache = useCache;
            this._useScope = useScope;
            this._errorTransforms = errorTransforms;
            this._invalidArgumendText = invalidArgumendText;
        }

        #region Public Methods

        /// <summary>
        /// Executed command asynchronous.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public async Task<CommandResponse> DispatchAsync(ICommand command)
        {
            using IServiceScope scope = this._provider.CreateScope();
            return await this.DispatchAsync(scope.ServiceProvider, command);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public async Task<CommandResponse> DispatchAsync(IServiceProvider provider, ICommand command)
        {
            //
            // before execute command search if has validator and executed.
            object sharedCache;
            if (this._validate)
            {
                Type commandValidationScopeType = typeof(CommandSharedCache<>).MakeGenericType(command.GetType());
                ICommandSharedCache commandValidationCache = Activator.CreateInstance(commandValidationScopeType, args: command) as ICommandSharedCache;

                var response = await ValidateAsync(provider, commandValidationCache, this._errorTransforms, this._invalidArgumendText);
                if (!response.IsSuccess)
                    return response;

                sharedCache = commandValidationCache.Cache;
            } else
                sharedCache = null;

            //
            // before execute command search if has compliance and executed
            CommandResponse complianceResp = await CheckAndExecuteCommandComplianceAsync(provider, command, sharedCache);
            if (!complianceResp.IsSuccess)
                return complianceResp;

            //
            // get handler and execute command.
            Type commandType = command.GetType();
            ICommandHandler handler = GetHandler(provider, command.GetType());
            if (this._useCache)
            {
                var method = GetFromCache(commandType, handler, true);
                return await (Task<CommandResponse>)method.Invoke(handler, new object[] { command, sharedCache });
            }

            return await ((dynamic)handler).HandleAsync((dynamic)command, sharedCache);
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="handler"></param>
        /// <param name="isAsync"></param>
        /// <returns></returns>
        private static MethodInfo GetFromCache(Type type, object handler, bool isAsync)
        {
            if (!_cache.TryGetValue(type, out MethodInfo method))
                lock (_sync)
                {
                    if (!_cache.ContainsKey(type))
                    {
                        method = handler
                            .GetType()
                            .GetMethod(nameof(ICommandHandler<ICommand>.HandleAsync), new Type[] { type, typeof(object) });
                        if (method == null)
                            throw new KeyNotFoundException($"Not found command handler, is Async: {isAsync} for {handler}");

                        _cache.Add(type, method);
                    }
                }
            return method;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="scopeProvider"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        private static ICommandHandler GetHandler(IServiceProvider scopeProvider, Type command)
        {
            Type serviceType = typeof(ICommandHandler<>).MakeGenericType(command);
            ICommandHandler commandHandler = (ICommandHandler)scopeProvider.GetService(serviceType);
            if (commandHandler == null)
                throw new KeyNotFoundException("There is no handler associated with this command");

            return commandHandler;
        }

        /// <summary>
        /// Register CommandCompliance in DPI.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="commandComplianceInterface">Interface used to register tha binding between CommandCompliance and command.</param>
        /// <param name="complianceAssembly"></param>
        public static void RegisterCommandCompliance(IServiceCollection services, Type commandComplianceInterface, IEnumerable<Assembly> complianceAssembly)
        {
            List<Type> existCommandCompliance = new List<Type>();
            foreach (var assembly in complianceAssembly)
            {
                var query = assembly
                    .GetTypes()
                    .Where(p => p.IsClass && p.IsAbstract == false && p.GetInterfaces().Contains(typeof(ICommandCompliance)));
                existCommandCompliance.AddRange(query);
            }

            foreach (var commandComplianceImplementation in existCommandCompliance)
            {
                var commandComplianceImplementedInterfaces = commandComplianceImplementation.GetInterfaces()
                    .Where(p =>
                        p.IsGenericType &&
                        p.GetGenericArguments().Length == 1 &&
                        p.GetGenericTypeDefinition() == commandComplianceInterface);

                foreach (var complianceInterface in commandComplianceImplementedInterfaces)
                {
                    var commandType = complianceInterface.GetGenericArguments().Single();
                    var wellKnowCommandComplianceInterface = typeof(ICommandCompliance<>).MakeGenericType(commandType);

                    services.AddScoped(wellKnowCommandComplianceInterface, commandComplianceImplementation);
                }
            }
        }
        /// <summary>
        /// Register CommandHandler and CommandValidation in DPI.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="commandHandlerInterface">Interface used to register tha binding between command handler and command.</param>
        /// <param name="commandAssembly"></param>
        /// <param name="validatorAssembly"></param>
        public static void RegisterCommandHandler(IServiceCollection services, Type commandHandlerInterface, IEnumerable<Assembly> commandAssembly, IEnumerable <Assembly> validatorAssembly = null)
        {
            Dictionary<Type, Type> cache = new Dictionary<Type, Type>();
            if (validatorAssembly != null)
            {
                List<AssemblyScanner> aux = new List<AssemblyScanner>();
                foreach (var assembly in validatorAssembly)
                {
                    foreach (var entry in AssemblyScanner.FindValidatorsInAssembly(assembly))
                        cache.Add(entry.InterfaceType, entry.ValidatorType);
                }
            }


            List<Type> existCommandHandler = new List<Type>();
            foreach (var assembly in commandAssembly)
            {
                var query = assembly
                    .GetTypes()
                    .Where(p => p.IsClass && p.IsAbstract == false && p.GetInterfaces().Contains(typeof(ICommandHandler)));
                existCommandHandler.AddRange(query);
            }

            foreach (var commandHandlerImplementation in existCommandHandler)
            {
                var commandHandlerImplementedInterfaces = commandHandlerImplementation.GetInterfaces()
                    .Where(p =>
                        p.IsGenericType &&
                        p.GetGenericArguments().Length == 1 &&
                        p.GetGenericTypeDefinition() == commandHandlerInterface);

                foreach (var handlerInterface in commandHandlerImplementedInterfaces)
                {
                    var commandType = handlerInterface.GetGenericArguments().Single();
                    var wellKnowCommandHandlerInterface = typeof(ICommandHandler<>).MakeGenericType(commandType);

                    services.AddScoped(wellKnowCommandHandlerInterface, commandHandlerImplementation);
                    if (validatorAssembly != null)
                    {
                        Type t1 = typeof(CommandSharedCache<>).MakeGenericType(commandType);
                        //
                        // add command associate validation 
                        Type validationInterfaceType = typeof(IValidator<>).MakeGenericType(t1);
                        if (cache.TryGetValue(validationInterfaceType, out Type validatorType))
                            services.AddScoped(validationInterfaceType, validatorType);
                    }
                }
            }
        }

        /// <summary>
        /// Check if exist compliance asociate to this command and executed.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="command"></param>
        /// <param name="sharedCache"></param>
        /// <returns></returns>
        public static async Task<CommandResponse> CheckAndExecuteCommandComplianceAsync(IServiceProvider provider, ICommand command, object sharedCache)
        {
            Type commandType = command.GetType();
            Type commandComplianceType = typeof(ICommandCompliance<>).MakeGenericType(commandType);

            ICommandCompliance commandCompliance = (ICommandCompliance)provider.GetService(commandComplianceType);
            if (commandCompliance != null)
                return await commandCompliance.ExecuteAsync(command, sharedCache);
            return command.OkResponse(true);
        }
        /// <summary>
        /// Find correct validator associate with the command and executed.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="commandValidationCache"></param>
        /// <param name="errorTransforms"></param>
        /// <param name="invalidArgumendText">Default text used to response in Inotify object when validation not success.</param>
        /// <returns></returns>
        public static async Task<CommandResponse> ValidateAsync(IServiceProvider provider, ICommandSharedCache commandValidationCache, Func<IList<ValidationFailure>, IDictionary<string, IEnumerable<string>>> errorTransforms = null, string invalidArgumendText = null)
        {
            var commandValidationScopeType = commandValidationCache.GetType();
            var validatorType = typeof(IValidator<>).MakeGenericType(commandValidationScopeType);
            if (provider.GetService(validatorType) is IValidator validator)
            {
                Type effectiveValidatorType = validator.GetType();
                var attribute = effectiveValidatorType.GetCustomAttribute<CacheTypeAttribute>();
                //
                // Assing cache type only if has one associate.
                if (attribute != null)
                    commandValidationCache.Cache = Activator.CreateInstance(attribute.CacheType);

                var valContext = new ValidationContext<ICommandSharedCache>(commandValidationCache);
                var errors = await validator.ValidateAsync(valContext);
                if (errors?.IsValid == false)
                {
                    if (errorTransforms == null)
                        return commandValidationCache.Get().BadResponse(errors.Errors, invalidArgumendText);
                    return commandValidationCache.Get().BadResponse(errorTransforms(errors.Errors), invalidArgumendText);
                }
            }
            return commandValidationCache.Get().OkResponse(true);
        }

        #endregion
    }
}
