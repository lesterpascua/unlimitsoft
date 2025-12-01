using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.Mediator.Validation;
using UnlimitSoft.Message;

namespace UnlimitSoft.Mediator;


/// <summary>
/// 
/// </summary>
public sealed class ServiceProviderMediator : IMediator
{
    private readonly bool _validate, _useScope;
    private readonly IServiceProvider _provider;
    private readonly Func<IEnumerable<ValidationFailure>, IDictionary<string, string[]>> _errorTransforms;

    private readonly ILogger<ServiceProviderMediator>? _logger;


    /// <summary>
    /// 
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="validate"></param>
    /// <param name="useScope"></param>
    /// <param name="errorTransforms"></param>
    /// <param name="logger"></param>
    public ServiceProviderMediator(
        IServiceProvider provider,
        bool validate = true,
        bool useScope = true,
        Func<IEnumerable<ValidationFailure>, IDictionary<string, string[]>>? errorTransforms = null,
        ILogger<ServiceProviderMediator>? logger = null
    )
    {
        _provider = provider;
        _validate = validate;
        _useScope = useScope;
        _errorTransforms = errorTransforms ?? DefaultErrorTransforms;
        _logger = logger;
    }

    /// <inheritdoc />
    public async ValueTask<Result<TResponse>> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken ct = default)
    {
        if (!_useScope)
            return await SendAsync(_provider, request, ct);

        using var scope = _provider.CreateScope();
        return await SendAsync(scope.ServiceProvider, request, ct);
    }
    /// <inheritdoc />
    public async ValueTask<Result<TResponse>> SafeSendAsync<TResponse>(IRequest<Result<TResponse>> request, CancellationToken ct = default)
    {
        if (!_useScope)
            return await SafeSendAsync(_provider, request, ct);

        using var scope = _provider.CreateScope();
        return await SafeSendAsync(scope.ServiceProvider, request, ct);
    }
    /// <inheritdoc />
    public async ValueTask<AgregateResult<TResponse>> PublishAsync<TResponse>(IRequest<TResponse> request, CancellationToken ct = default)
    {
        if (!_useScope)
            return await PublishAsync(_provider, request, ct);

        using var scope = _provider.CreateScope();
        return await PublishAsync(scope.ServiceProvider, request, ct);
    }

    /// <inheritdoc />
    public ValueTask<Result<TResponse>> SendAsync<TResponse>(IServiceProvider provider, IRequest<TResponse> request, CancellationToken ct = default) => InternalSendAsync<TResponse, TResponse>(provider, request, ct);
    /// <inheritdoc />
    public ValueTask<Result<TResponse>> SafeSendAsync<TResponse>(IServiceProvider provider, IRequest<Result<TResponse>> request, CancellationToken ct = default) => InternalSendAsync<Result<TResponse>, TResponse>(provider, request, ct);
    /// <inheritdoc />
    public async ValueTask<AgregateResult<TResponse>> PublishAsync<TResponse>(IServiceProvider provider, IRequest<TResponse> request, CancellationToken ct = default)
    {
        _logger?.LogProcessRequest(request);

        //
        // Get handler and execute command.
        var requestType = request.GetType();
        var responseType = typeof(TResponse);
        _logger?.LogExecuteRequestType(requestType, responseType);

        var handlers = GetRequestHandlers(provider, requestType, responseType, out var metadatas);
        if (handlers is null)
            return new AgregateResult<TResponse>([Result.Err<TResponse>(request.NotFoundResponse())]);

        var index = 0;
        var responses = new Result<TResponse>[handlers.Length];
        for (var i = 0; i < handlers.Length; i++)
        {
            var handler = handlers[i];
            var metadata = metadatas![index];

            if (metadata.HasLifeCycle)
                await InitAsync(handler, request, requestType, metadata, ct);

            try
            {
                if (_validate)
                {
                    if (metadata.Validator is not null)
                    {
                        var error = await ValidationAsync(handler, request, requestType, metadata, ct);
                        if (error is not null)
                        {
                            responses[i] = Result.Err<TResponse>(error);
                            continue;
                        }
                    }
                    if (metadata.HasCompliance)
                    {
                        var error = await ComplianceAsync(handler, request, requestType, metadata, ct);
                        if (error is not null)
                        {
                            responses[i] = Result.Err<TResponse>(error);
                            continue;
                        }
                    }
                }
                var response = await HandlerAsync(handler, request, requestType, metadata, ct);
                if (response is IResult result)
                {
                    responses[i] = result.IsSuccess ? Result.Ok((TResponse)result.GetValue()!) : Result.Err<TResponse>(result.Error!);
                    continue;
                }
                responses[i] = Result.Ok(response);
            }
            finally
            {
                if (metadata.HasLifeCycle)
                    await EndAsync(handler, request, requestType, metadata, ct);
            }
            index++;
        }
        return new AgregateResult<TResponse>(responses);
    }

    /// <summary>
    /// Default function used to convert error transform to standard ASP.NET format
    /// </summary>
    public static IDictionary<string, string[]> DefaultErrorTransforms(IEnumerable<ValidationFailure> failure)
    {
        var aux = new Dictionary<string, List<string>>();
        foreach (var fail in failure)
        {
            if (!aux.TryGetValue(fail.PropertyName, out var list))
                aux.Add(fail.PropertyName, list = new List<string>());
            list.Add(fail.ErrorMessage);
        }

        var result = new Dictionary<string, string[]>();
        foreach (var item in aux)
            result.Add(item.Key, [.. item.Value]);

        return result;
    }


    #region Private Methods
    private static IRequestHandler? GetRequestHandler(IServiceProvider provider, Type requestType, Type responseType, out RequestMetadata? metadata)
    {
        if (InvokerCache.Cache.TryGetValue(requestType, out var metadatas))
        {
            metadata = metadatas[0];
            if (metadata.HandlerInterfaceType is not null)
                return (IRequestHandler? )provider.GetService(metadatas[0].HandlerInterfaceType);
            return null;
        }

        var handlers = BuildRequestHandlerCache(provider, requestType, responseType, out metadatas);
        if (handlers is not null)
        {
            metadata = metadatas![0];
            return handlers?[0];
        }

        metadata = null;
        return null;
    }
    private static IRequestHandler[]? GetRequestHandlers(IServiceProvider provider, Type requestType, Type responseType, out RequestMetadata[]? metadatas)
    {
        if (InvokerCache.Cache.TryGetValue(requestType, out metadatas))
        {
            if (metadatas[0].HandlerInterfaceType is not null)
            {
                var handlers = (IRequestHandler[])provider.GetServices(metadatas[0].HandlerInterfaceType);
                if (handlers.Length != 0)
                    return handlers;
            }
            return null;
        }
        return BuildRequestHandlerCache(provider, requestType, responseType, out metadatas);
    }
    private static IRequestHandler[]? BuildRequestHandlerCache(IServiceProvider provider, Type requestType, Type responseType, out RequestMetadata[]? metadatas)
    {
        // Handler
        lock (InvokerCache.Sync)
        {
            if (InvokerCache.Cache.TryGetValue(requestType, out metadatas!))
                return (IRequestHandler[])provider.GetServices(metadatas[0].HandlerInterfaceType);             // Resolve service

            // Create Handler
            var handlerInterfaceType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);
            var handlers = (IRequestHandler[])provider.GetServices(handlerInterfaceType);
            if (handlers.Length == 0)
            {
                InvokerCache.UnsafeAdd(requestType, [RequestMetadata.Empty ??= new()]);
                return null;
            }

            // Pipeline
            var pipeline = GetPipeline(provider, requestType, responseType);

            metadatas = new RequestMetadata[handlers.Length];
            for (var i = 0; i < handlers.Length; i++)
            {
                var handler = handlers[i];

                // Features implemented by this command
                var handleType = handler.GetType();
                var metadata = metadatas[i] = new RequestMetadata
                {
                    HandlerInterfaceType = handlerInterfaceType,
                    HandlerImplementType = handleType,
                    Pipeline = pipeline,
                };
                var interfaces = handleType.GetInterfaces();

                // LiveCycle
                var liveCycleHandlerType = typeof(IRequestHandlerLifeCycle<>).MakeGenericType(requestType);
                if (Array.IndexOf(interfaces, liveCycleHandlerType) != -1)
                    metadata.HasLifeCycle = true;

                // Validator
                var validationHandlerType = typeof(IRequestHandlerValidator<>).MakeGenericType(requestType);
                if (Array.IndexOf(interfaces, validationHandlerType) != -1)
                    metadata.Validator = typeof(RequestValidator<>).MakeGenericType(requestType);

                // Compliance
                var complianceHandlerType = typeof(IRequestHandlerCompliance<>).MakeGenericType(requestType);
                if (Array.IndexOf(interfaces, complianceHandlerType) != -1)
                    metadata.HasCompliance = true;
            }

            InvokerCache.UnsafeAdd(requestType, metadatas);
            return handlers;
        }

        // ==========================================================================================================================
        static RequestMetadata.PipelineM? GetPipeline(IServiceProvider provider, Type requestType, Type responseType)
        {
            var pipelineHandlerType = typeof(IRequestHandlerPipeline<,>).MakeGenericType(requestType, responseType);
            var pipelinesServices = (IRequestHandlerPipeline[])provider.GetServices(pipelineHandlerType);
            if (pipelinesServices.Length == 0)
                return null;

            return new RequestMetadata.PipelineM
            {
                InterfaceType = pipelineHandlerType,
                Handlers = [.. pipelinesServices
                    .OrderBy(x => x.Order)
                    .Select(s => new RequestMetadata.PipelineM.Impl { ImplementType = s.GetType(), })
                ]
            };
        }
    }

    private async ValueTask<Result<TOut>> InternalSendAsync<TResponse, TOut>(IServiceProvider provider, IRequest<TResponse> request, CancellationToken ct)
    {
        _logger?.LogProcessRequest(request);

        //
        // Get handler and execute command.
        var requestType = request.GetType();
        var responseType = typeof(TResponse);
        _logger?.LogExecuteRequestType(requestType, responseType);

        var handler = GetRequestHandler(provider, requestType, responseType, out var metadata);
        if (handler is null || metadata is null)
            return Result.Err<TOut>(request.NotFoundResponse());

        var pipelineHandlers = metadata.Pipeline is not null ? await PrePipelineAsync(provider, request, metadata, ct) : null;

        if (metadata.HasLifeCycle)
            await InitAsync(handler, request, requestType, metadata, ct);

        TResponse response = default!;
        try
        {
            if (_validate)
            {
                if (metadata.Validator is not null)
                {
                    var error = await ValidationAsync(handler, request, requestType, metadata, ct).ConfigureAwait(false);
                    if (error is not null)
                        return Result.Err<TOut>(error);
                }
                if (metadata.HasCompliance)
                {
                    var error = await ComplianceAsync(handler, request, requestType, metadata, ct).ConfigureAwait(false);
                    if (error is not null)
                        return Result.Err<TOut>(error);
                }
            }
            response = await HandlerAsync(handler, request, requestType, metadata, ct).ConfigureAwait(false);
        }
        finally
        {
            if (metadata.HasLifeCycle)
                await EndAsync(handler, request, requestType, metadata, ct);
            if (pipelineHandlers is not null)
                response = await PostPipelineAsync(request, pipelineHandlers, response, ct);
        }

        if (response is null)
            return Result.Ok<TOut>(default);

        if (response is Result<TOut> result)
        {
            if (result.IsSuccess)
                return Result.Ok(result.Value);
            return Result.Err<TOut>(result.Error!);
        }
        return Result.Ok((TOut)(object)response);
    }


    private static async ValueTask<IRequestHandlerPipeline[]?> PrePipelineAsync<TResponse>(IServiceProvider provider, IRequest<TResponse> request, RequestMetadata metadata, CancellationToken ct)
    {
        /// TODO: pending to optimize with InvokerCache
        var pipelineHandlers = new IRequestHandlerPipeline[metadata.Pipeline!.Handlers.Length];
        for (var i = 0; i < metadata.Pipeline.Handlers.Length; i++)
        {
            var pipelineInfo = metadata.Pipeline.Handlers[i];
            var pipelineHandler = pipelineHandlers[i] = (IRequestHandlerPipeline)ActivatorUtilities.GetServiceOrCreateInstance(provider, pipelineInfo.ImplementType);
            var method = pipelineInfo.ImplementType.GetMethod(nameof(IRequestHandlerPipeline<,>.PreHandleAsync))!;
            await (ValueTask)method.Invoke(pipelineHandler, [request, ct])!;
        }

        return pipelineHandlers;
    }
    private static ValueTask<TResponse> HandlerAsync<TResponse>(IRequestHandler handler, IRequest<TResponse> request, Type requestType, RequestMetadata metadata, CancellationToken ct)
    {
        var cli = metadata.HandlerCLI;
        if (cli is not null)
            return ((Func<IRequestHandler, IRequest<TResponse>, CancellationToken, ValueTask<TResponse>>)cli).Invoke(handler, request, ct);

        var method = InvokerCache.GetHandler<TResponse>(requestType, metadata);
        return method(handler, request, ct);
    }
    private async ValueTask<IResponse?> ValidationAsync(IRequestHandler handler, IRequest request, Type requestType, RequestMetadata metadata, CancellationToken ct)
    {
        _logger?.LogRequestHandlerImplementInternalValidation(request);

        var validator = (IValidator)Activator.CreateInstance(metadata.Validator!)!;
        var method = InvokerCache.GetValidator(requestType, metadata);

        var response = await method(handler, request, validator, ct);
        if (!response.IsSuccess)
            return response;

        var valContext = new ValidationContext<IRequest>(request);
        var errors = await validator.ValidateAsync(valContext, ct);

        _logger?.LogEvaluateValidatorProcessResult(errors);
        if (errors?.IsValid != false)
            return null;

        var aux = _errorTransforms(errors.Errors);
        return request.BadResponse(aux);
    }
    private async ValueTask<IResponse?> ComplianceAsync(IRequestHandler handler, IRequest request, Type requestType, RequestMetadata metadata, CancellationToken ct)
    {
        _logger?.LogRequestHandlerImplementInternalCompliance(request);

        var method = InvokerCache.GetCompliance(requestType, metadata);
        var response = await method(handler, request, ct);
        if (!response.IsSuccess)
            return response;

        return null;
    }
    private ValueTask InitAsync<TResponse>(IRequestHandler handler, IRequest<TResponse> request, Type requestType, RequestMetadata metadata, CancellationToken ct)
    {
        _logger?.LogRequestHandlerImplementInternalInitLifeCycle(request);

        var method = InvokerCache.GetInit(requestType, metadata);
        return method(handler, request, ct);
    }
    private ValueTask EndAsync<TResponse>(IRequestHandler handler, IRequest<TResponse> request, Type requestType, RequestMetadata metadata, CancellationToken ct)
    {
        _logger?.LogRequestHandlerImplementInternalEndLifeCycle(request);

        var method = InvokerCache.GetEnd(requestType, metadata);
        return method(handler, request, ct);
    }
    private static async ValueTask<TResponse> PostPipelineAsync<TResponse>(IRequest<TResponse> request, IRequestHandlerPipeline[] pipelineHandlers, TResponse response, CancellationToken ct)
    {
        /// TODO: pending to optimize with InvokerCache
        for (var i = pipelineHandlers.Length - 1; i >= 0; i--)
        {
            var pipelineHandler = pipelineHandlers[i];
            var method = pipelineHandler.GetType().GetMethod(nameof(IRequestHandlerPipeline<,>.PostHandleAsync))!;
            response = await (ValueTask<TResponse>)method.Invoke(pipelineHandler, [request, response, ct])!;
        }

        return response;
    }
    #endregion
}
internal static partial class ServiceProviderMediatorLogger
{
    [LoggerMessage(EventId = 10, Message = "Process request: {Request}", Level = LogLevel.Debug, SkipEnabledCheck = true)]
    public static partial void LogProcessRequest(this ILogger logger, IRequest request);
    [LoggerMessage(EventId = 11, Message = "Execute request type {RequestType} with response {ResponseType}", Level = LogLevel.Debug, SkipEnabledCheck = true)]
    public static partial void LogExecuteRequestType(this ILogger logger, Type requestType, Type responseType);
    [LoggerMessage(EventId = 12, Message = "Request {Request} handler implement internal validation", Level = LogLevel.Debug, SkipEnabledCheck = true)] 
    public static partial void LogRequestHandlerImplementInternalValidation(this ILogger logger, IRequest request);
    [LoggerMessage(EventId = 13, Message = "Evaluate validator process result: {Errors}", Level = LogLevel.Debug, SkipEnabledCheck = true)]
    public static partial void LogEvaluateValidatorProcessResult(this ILogger logger, ValidationResult? errors);
    [LoggerMessage(EventId = 14, Message = "Request {Request} handler implement internal compliance", Level = LogLevel.Debug, SkipEnabledCheck = true)]
    public static partial void LogRequestHandlerImplementInternalCompliance(this ILogger logger, IRequest request);
    [LoggerMessage(EventId = 15, Message = "Request {Request} handler implement internal init life cycle", Level = LogLevel.Debug, SkipEnabledCheck = true)]
    public static partial void LogRequestHandlerImplementInternalInitLifeCycle(this ILogger logger, IRequest request);
    [LoggerMessage(EventId = 16, Message = "Request {Request} handler implement internal end life cycle", Level = LogLevel.Debug, SkipEnabledCheck = true)]
    public static partial void LogRequestHandlerImplementInternalEndLifeCycle(this ILogger logger, IRequest request);
}