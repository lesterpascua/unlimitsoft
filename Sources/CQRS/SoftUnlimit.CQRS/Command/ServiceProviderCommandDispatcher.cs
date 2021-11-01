using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftUnlimit.CQRS.Cache;
using SoftUnlimit.CQRS.Command.Validation;
using SoftUnlimit.CQRS.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Command
{
    /// <summary>
    /// Implement a command dispatcher resolving all handler by a ServiceProvider.
    /// </summary>
    public class ServiceProviderCommandDispatcher : CacheDispatcher, ICommandDispatcher
    {
        private readonly IServiceProvider _provider;
        private readonly bool _validate, _useCache, _useScope;
        private readonly Func<IList<ValidationFailure>, IDictionary<string, IEnumerable<string>>> _errorTransforms;
        private readonly string _invalidArgumendText;
        private readonly Action<IServiceProvider, ICommand> _preeDispatch;
        private readonly ILogger<ServiceProviderCommandDispatcher> _logger;

        /// <summary>
        /// Default function used to convert error transform to standard ASP.NET format
        /// </summary>
        public static readonly Func<IList<ValidationFailure>, IDictionary<string, IEnumerable<string>>> DefaultErrorTransforms = (failure) => failure.GroupBy(p => p.PropertyName).ToDictionary(k => k.Key, v => v.Select(s => s.ErrorMessage));


        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="validate">Enable command validation after execute associate handler.</param>
        /// <param name="useCache">Store reference method of command handler associate by command to better performance.</param>
        /// <param name="useScope">Create scope to resolve element in DPI.</param>
        /// <param name="errorTransforms">Conver error to a Dictionary where key is a propertyName with an error and value is all error description.</param>
        /// <param name="invalidArgumendText">Default text used to response in Inotify object when validation not success.</param>
        /// <param name="preeDispatch">Before dispatch command flow call this action.</param>
        /// <param name="logger"></param>
        public ServiceProviderCommandDispatcher(IServiceProvider provider, bool validate = true, bool useCache = true,
            bool useScope = true, Func<IList<ValidationFailure>, IDictionary<string, IEnumerable<string>>> errorTransforms = null, 
            string invalidArgumendText = null, Action<IServiceProvider, ICommand> preeDispatch = null, ILogger<ServiceProviderCommandDispatcher> logger = null
        )
            : base(useCache)
        {
            _provider = provider;
            _validate = validate;
            _useCache = useCache;
            _useScope = useScope;
            _errorTransforms = errorTransforms;
            _invalidArgumendText = invalidArgumendText;
            _preeDispatch = preeDispatch;
            _logger = logger;
        }

        #region Public Methods

        /// <inheritdoc />
        public async Task<ICommandResponse> DispatchAsync(ICommand command, CancellationToken ct)
        {
            if (!_useScope)
                return await DispatchAsync(_provider, command, ct);

            using IServiceScope scope = _provider.CreateScope();
            return await DispatchAsync(scope.ServiceProvider, command, ct);
        }
        /// <inheritdoc />
        public async Task<ICommandResponse> DispatchAsync(IServiceProvider provider, ICommand command, CancellationToken ct)
        {
            _logger?.LogDebug("Invoke pree dispatch method.");
            _preeDispatch?.Invoke(provider, command);

            //
            // get handler and execute command.
            Type commandType = command.GetType();
            _logger?.LogDebug("Execute command type: {Type}", commandType);

            var handler = GetCommandHandler(provider, command.GetType());

            if (_validate)
            {
                var interfaces = handler.GetType().GetInterfaces();

                #region Verify if command implement internal validation
                var validationHandlerType = typeof(ICommandHandlerValidator<>).MakeGenericType(commandType);
                if (interfaces.Any(type => type == validationHandlerType))
                {
                    _logger?.LogDebug("Command handler implement internal validation");

                    var validatorType = typeof(CommandValidator<>).MakeGenericType(commandType);
                    IValidator validator = (IValidator)Activator.CreateInstance(validatorType);
                    validator = await (ValueTask<IValidator>)((dynamic)handler).ValidatorAsync((dynamic)command, (dynamic)validator, ct);

                    var valContext = new ValidationContext<ICommand>(command);
                    var errors = await validator.ValidateAsync(valContext, ct);

                    _logger?.LogDebug("Evaluate validator process result: {@Errors}", errors);
                    if (errors?.IsValid == false)
                    {
                        if (_errorTransforms == null)
                            return command.BadResponse(errors.Errors, _invalidArgumendText);
                        return command.BadResponse(_errorTransforms(errors.Errors), _invalidArgumendText);
                    }
                }
                else
                    _logger?.LogDebug("Command not handler implement internal validation");
                #endregion

                #region Verify if command implement internal compliance
                var complianceHandlerType = typeof(ICommandHandlerCompliance<>).MakeGenericType(commandType);
                if (interfaces.Any(type => type == complianceHandlerType))
                {
                    _logger?.LogDebug("Command handler implement internal compliance");

                    var response = await (Task<ICommandResponse>)((dynamic)handler).HandleComplianceAsync((dynamic)command, ct);
                    if (!response.IsSuccess)
                        return response;
                }
                else
                    _logger?.LogDebug("Command not handler implement internal compliance");
                #endregion
            }
            return await ExecuteHandlerForCommandAsync(handler, command, commandType, UseCache, ct);
        }

        #endregion

        #region Static Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="scopeProvider"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        private static ICommandHandler GetCommandHandler(IServiceProvider scopeProvider, Type command)
        {
            Type serviceType = typeof(ICommandHandler<>).MakeGenericType(command);
            ICommandHandler commandHandler = (ICommandHandler)scopeProvider.GetService(serviceType);
            if (commandHandler == null)
                throw new KeyNotFoundException("There is no handler associated with this command");

            return commandHandler;
        }
        private static async Task<ICommandResponse> ExecuteHandlerForCommandAsync(ICommandHandler handler, ICommand command, Type commandType, bool useCache, CancellationToken ct)
        {
            Task<ICommandResponse> result;
            if (useCache)
            {
                var method = GetCommandHandlerFromCache(commandType, handler);
                result = method(handler, command, ct);
            }
            else
                result = (Task<ICommandResponse>)((dynamic)handler).HandleAsync((dynamic)command, ct);

            return await result;
        }
        #endregion
    }
}
