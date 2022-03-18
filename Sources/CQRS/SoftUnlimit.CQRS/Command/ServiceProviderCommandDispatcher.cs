using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftUnlimit.CQRS.Cache;
using SoftUnlimit.CQRS.Command.Validation;
using SoftUnlimit.CQRS.Logging;
using SoftUnlimit.CQRS.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Command
{
    /// <summary>
    /// Implement a command dispatcher resolving all handler by a ServiceProvider.
    /// </summary>
    public class ServiceProviderCommandDispatcher : ICommandDispatcher
    {
        private readonly IServiceProvider _provider;

        private readonly bool _validate, _useScope;
        private readonly Action<IServiceProvider, ICommand> _preeDispatch;

        private readonly string _invalidArgumendText;
        private readonly Func<IEnumerable<ValidationFailure>, IDictionary<string, string[]>> _errorTransforms;

        private readonly Dictionary<Type, CommandMetadata> _cache;
        private readonly ILogger<ServiceProviderCommandDispatcher> _logger;

        /// <summary>
        /// Default function used to convert error transform to standard ASP.NET format
        /// </summary>
        public static readonly Func<IEnumerable<ValidationFailure>, IDictionary<string, string[]>> DefaultErrorTransforms = (failure) => failure.GroupBy(p => p.PropertyName).ToDictionary(k => k.Key, v => v.Select(s => s.ErrorMessage).ToArray());


        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="validate">Enable command validation after execute associate handler.</param>
        /// <param name="useScope">Create scope to resolve element in DPI.</param>
        /// <param name="errorTransforms">Conver error to a Dictionary where key is a propertyName with an error and value is all error description.</param>
        /// <param name="invalidArgumendText">Default text used to response in Inotify object when validation not success.</param>
        /// <param name="preeDispatch">Before dispatch command flow call this action.</param>
        /// <param name="logger"></param>
        public ServiceProviderCommandDispatcher(IServiceProvider provider, bool validate = true,
            bool useScope = true, Func<IEnumerable<ValidationFailure>, IDictionary<string, string[]>> errorTransforms = null,
            string invalidArgumendText = null, Action<IServiceProvider, ICommand> preeDispatch = null, ILogger<ServiceProviderCommandDispatcher> logger = null
        )
        {
            _provider = provider;
            _validate = validate;
            _useScope = useScope;
            _errorTransforms = errorTransforms;
            _invalidArgumendText = invalidArgumendText;
            _preeDispatch = preeDispatch;
            _logger = logger;
            _cache = new Dictionary<Type, CommandMetadata>();
        }

        #region Public Methods

        /// <inheritdoc />
        public async Task<ICommandResponse> DispatchAsync(ICommand command, CancellationToken ct = default)
        {
            if (!_useScope)
                return await DispatchAsync(_provider, command, ct);

            using var scope = _provider.CreateScope();
            return await DispatchAsync(scope.ServiceProvider, command, ct);
        }
        /// <inheritdoc />
        public async Task<ICommandResponse> DispatchAsync(IServiceProvider provider, ICommand command, CancellationToken ct = default)
        {
            _preeDispatch?.Invoke(provider, command);
            _logger?.ProcessCommand(command);

            //
            // Get handler and execute command.
            var commandType = command.GetType();
            _logger?.ExecuteCommandType(commandType);

            var handler = GetCommandHandler(provider, command.GetType(), out var metadata);
            if (_validate)
            {
                ICommandResponse response;

                response = await RunValidationAsync(handler, command, commandType, metadata, ct);
                if (response is not null)
                    return response;

                response = await ComplianceAsync(handler, command, commandType, metadata, ct);
                if (response is not null)
                    return response;
            }

            return await HandlerAsync(handler, command, commandType, ct);
        }

        #endregion

        #region Static Methods
        /// <summary>
        /// Get command handler and metadata asociate to a command.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="commandType"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        private ICommandHandler GetCommandHandler(IServiceProvider provider, Type commandType, out CommandMetadata metadata)
        {
            if (_cache.TryGetValue(commandType, out metadata))
                return (ICommandHandler)provider.GetRequiredService(metadata.CommandHandler);

            lock (_cache)
                if (!_cache.TryGetValue(commandType, out metadata))
                {
                    _cache.Add(commandType, metadata = new CommandMetadata());

                    // Handler
                    metadata.CommandHandler = typeof(ICommandHandler<>).MakeGenericType(commandType);
                    var handler = (ICommandHandler)provider.GetRequiredService(metadata.CommandHandler);

                    // Features implemented by this command
                    var interfaces = handler.GetType().GetInterfaces();

                    // Validator
                    var validationHandlerType = typeof(ICommandHandlerValidator<>).MakeGenericType(commandType);
                    if (interfaces.Any(type => type == validationHandlerType))
                        metadata.ValidatorType = typeof(CommandValidator<>).MakeGenericType(commandType);

                    // Compliance
                    var complianceHandlerType = typeof(ICommandHandlerCompliance<>).MakeGenericType(commandType);
                    if (interfaces.Any(type => type == complianceHandlerType))
                        metadata.HasCompliance = true;

                    return handler;
                }

            // Resolve service
            return (ICommandHandler)provider.GetRequiredService(metadata.CommandHandler);
        }
        /// <summary>
        /// Execute command handler
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="command"></param>
        /// <param name="commandType"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private Task<ICommandResponse> HandlerAsync(ICommandHandler handler, ICommand command, Type commandType, CancellationToken ct)
        {
            var method = CacheDispatcher.GetCommandHandler(commandType, handler);
            return method(handler, command, ct);
        }
        /// <summary>
        /// Execute command compliance
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="command"></param>
        /// <param name="commandType"></param>
        /// <param name="metadata"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async ValueTask<ICommandResponse> ComplianceAsync(ICommandHandler handler, ICommand command, Type commandType, CommandMetadata metadata, CancellationToken ct)
        {
            if (!metadata.HasCompliance)
            {
                _logger?.CommandNotHandlerImplementCompliance(command);
                return null;
            }

            _logger?.CommandNotHandlerImplementCompliance(command);

            var method = CacheDispatcher.GetCommandCompliance(commandType, handler);
            var response = await method(handler, command, ct);
            if (!response.IsSuccess)
                return response;

            return null;
        }
        /// <summary>
        /// Execute validator
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="command"></param>
        /// <param name="commandType"></param>
        /// <param name="metadata"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async ValueTask<ICommandResponse> RunValidationAsync(ICommandHandler handler, ICommand command, Type commandType, CommandMetadata metadata, CancellationToken ct)
        {
            if (metadata.ValidatorType is null)
            {
                _logger?.CommandNotHandlerImplementValidation(command);
                return null;
            }

            _logger?.CommandHandlerImplementValidation(command);

            var validator = (IValidator)Activator.CreateInstance(metadata.ValidatorType);
            var method = CacheDispatcher.GetCommandValidator(commandType, metadata.ValidatorType, handler);

            await method(handler, command, validator, ct);

            var valContext = new ValidationContext<ICommand>(command);
            var errors = await validator.ValidateAsync(valContext, ct);

            _logger?.EvaluateValidatorProcessResultErrors(errors);
            if (errors?.IsValid != false)
                return null;

            if (_errorTransforms is null)
                return command.BadResponse(errors.Errors, _invalidArgumendText);
            return command.BadResponse(_errorTransforms(errors.Errors), _invalidArgumendText);
        }
        #endregion

        #region Nested Classes
        private sealed class CommandMetadata
        {
            public Type ValidatorType;
            public Type CommandHandler;
            public bool HasCompliance;
        }
        #endregion
    }
}
