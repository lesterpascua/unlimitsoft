using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.Mediator.Pipeline;
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
    public ValueTask<Result<TResponse>> SendAsync<TResponse>(IServiceProvider provider, IRequest<TResponse> request, CancellationToken ct = default)
    {
        return InternalSendAsync(
            provider,
            request,
            Transform,
            ct
        );
    }
    /// <inheritdoc />
    public ValueTask<Result<TResponse>> SafeSendAsync<TResponse>(IServiceProvider provider, IRequest<Result<TResponse>> request, CancellationToken ct = default)
    {
        return InternalSendAsync(
            provider,
            request,
            TransformWithResult,
            ct
        );
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
            result.Add(item.Key, item.Value.ToArray());

        return result;
    }


    #region Private Methods
    private static IRequestHandler? BuildHandlerMetadata(IServiceProvider provider, Type requestType, Type responseType, out RequestMetadata metadata)
    {
        if (InvokerCache.Cache.TryGetValue(requestType, out metadata!))
        {
            if (metadata.HandlerInterfaceType is not null)
                return (IRequestHandler)provider.GetRequiredService(metadata.HandlerInterfaceType);
            return null;
        }

        // Handler
        lock (InvokerCache.Cache)
        {
            if (InvokerCache.Cache.TryGetValue(requestType, out metadata!))
                return (IRequestHandler)provider.GetRequiredService(metadata.HandlerInterfaceType);             // Resolve service

            // Create Handler
            var handlerInterfaceType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);
            var handler = (IRequestHandler?)provider.GetService(handlerInterfaceType);
            if (handler is null)
            {
                InvokerCache.Cache.Add(requestType, new RequestMetadata());
                return null;
            }

            // Features implemented by this command
            var handleType = handler.GetType();
            var parameters = handlerInterfaceType.GetGenericArguments();
            metadata = new RequestMetadata
            {
                HandlerInterfaceType = handlerInterfaceType,
                HandlerImplementType = handleType,

                IsResult = parameters[1].GetGenericTypeDefinition() == typeof(Result<>),
            };
            var interfaces = handleType.GetInterfaces();

            // LiveCycle
            var liveCycleHandlerType = typeof(IRequestHandlerLifeCycle<>).MakeGenericType(requestType);
            if (interfaces.Any(type => type == liveCycleHandlerType))
                metadata.HasLifeCycle = true;

            // Validator
            var validationHandlerType = typeof(IRequestHandlerValidator<>).MakeGenericType(requestType);
            if (interfaces.Any(type => type == validationHandlerType))
                metadata.Validator = typeof(RequestValidator<>).MakeGenericType(requestType);

            // Compliance
            var complianceHandlerType = typeof(IRequestHandlerCompliance<>).MakeGenericType(requestType);
            if (interfaces.Any(type => type == complianceHandlerType))
                metadata.HasCompliance = true;

            // PostPipeline
            var attrs = requestType.GetCustomAttributes(typeof(IPostPipelineAttribute), true);
            if (attrs is not null && attrs.Length != 0)
            {
                var list = new List<(Type Pipeline, int Order)>();
                var requestHandlerType = typeof(IRequestHandlerPostPipeline<,,,>);
                for (var i = 0; i < attrs.Length; i++)
                {
                    var attr = (IPostPipelineAttribute)attrs[i];
                    var type = requestHandlerType.MakeGenericType(requestType, handleType, responseType, attr.Pipeline);
                    var collection = attr.Pipeline.GetInterfaces()
                        .Where(p => p == type)
                        .Select(s => (Type: type, attr.Order));

                    list.AddRange(collection);
                }
                metadata.PostPipeline = list
                    .GroupBy(k => k.Order, s => s.Pipeline)
                    .OrderBy(k => k.Key)
                    .Select(type => type.Select(s => new PostPipelineMetadata { InterfaceType = s }).ToArray())
                    .ToArray();
            }

            InvokerCache.Cache.Add(requestType, metadata);
            return handler;
        }
    }


    private static Result<TOut> Transform<TOut>(TOut result)
    {
        return Result.FromOk(result);
    }
    private static Result<TOut> TransformWithResult<TOut>(Result<TOut> result)
    {
        if (result.IsSuccess)
            return Result.FromOk(result.Value);
        return Result.FromError<TOut>(result.Error!);
    }
    private async ValueTask<Result<TOut>> InternalSendAsync<TResponse, TOut>(IServiceProvider provider, IRequest<TResponse> request, Func<TResponse?, Result<TOut>> transform, CancellationToken ct)
    {
        _logger?.LogDebug(10, "Process request: {@Request}", request);

        //
        // Get handler and execute command.
        var requestType = request.GetType();
        var responseType = typeof(TResponse);
        _logger?.LogDebug(12, "Execute request type {Request} with response {Response}", requestType, responseType);

        var handler = BuildHandlerMetadata(provider, requestType, responseType, out var metadata);
        if (handler is null)
            return Result.FromError<TOut>(request.NotFoundResponse());

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
                        return Result.FromError<TOut>(error);
                }
                if (metadata.HasCompliance)
                {
                    var error = await ComplianceAsync(handler, request, requestType, metadata, ct);
                    if (error is not null)
                        return Result.FromError<TOut>(error);
                }
            }
            var response = await HandlerAsync(handler, request, requestType, metadata, ct);

            // Run existing post operations
            if (metadata.PostPipeline is not null)
                PostPipelineHandlerAsync(provider, requestType, request, handler, response, metadata, ct);

            return transform(response);
        }
        finally
        {
            if (metadata.HasLifeCycle)
                await EndAsync(handler, request, requestType, metadata, ct);
        }
    }


    private static async ValueTask<TResponse?> HandlerAsync<TResponse>(IRequestHandler handler, IRequest<TResponse> request, Type requestType, RequestMetadata metadata, CancellationToken ct)
    {
        var method = InvokerCache.GetHandler<TResponse>(requestType, metadata);
        return await method(handler, request, ct);
    }
    /// <summary>
    /// Execute validator
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="request"></param>
    /// <param name="requestType"></param>
    /// <param name="metadata"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    private async ValueTask<IResponse?> ValidationAsync(IRequestHandler handler, IRequest request, Type requestType, RequestMetadata metadata, CancellationToken ct)
    {
        _logger?.LogDebug("Request {Request} handler implement internal validation", request);

        var validator = (IValidator)Activator.CreateInstance(metadata.Validator!)!;
        var method = InvokerCache.GetValidator(requestType, metadata);

        var response = await method(handler, request, validator, ct);
        if (!response.IsSuccess)
            return response;

        var valContext = new ValidationContext<IRequest>(request);
        var errors = await validator.ValidateAsync(valContext, ct);

        _logger?.LogDebug(11, "Evaluate validator process result: {@Errors}", errors);
        if (errors?.IsValid != false)
            return null;

        var aux = _errorTransforms(errors.Errors);
        return request.BadResponse(aux);
    }
    private async ValueTask<IResponse?> ComplianceAsync(IRequestHandler handler, IRequest request, Type requestType, RequestMetadata metadata, CancellationToken ct)
    {
        _logger?.LogDebug("Request {Request} handler implement internal compliance", request);

        var method = InvokerCache.GetCompliance(requestType, metadata);
        var response = await method(handler, request, ct);
        if (!response.IsSuccess)
            return response;

        return null;
    }
    private ValueTask InitAsync<TResponse>(IRequestHandler handler, IRequest<TResponse> request, Type requestType, RequestMetadata metadata, CancellationToken ct)
    {
        _logger?.LogDebug("Request {Request} handler implement internal life cycle", request);

        var method = InvokerCache.GetInit(requestType, metadata);
        return method(handler, request, ct);
    }
    private ValueTask EndAsync<TResponse>(IRequestHandler handler, IRequest<TResponse> request, Type requestType, RequestMetadata metadata, CancellationToken ct)
    {
        _logger?.LogDebug("Request {Request} handler implement internal life cycle", request);

        var method = InvokerCache.GetEnd(requestType, metadata);
        return method(handler, request, ct);
    }

    /// <summary>
    /// Handler post async operations
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="requestType"></param>
    /// <param name="request"></param>
    /// <param name="handler"></param>
    /// <param name="response"></param>
    /// <param name="metadata"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    private static void PostPipelineHandlerAsync<TResponse>(IServiceProvider provider, Type requestType, IRequest request, IRequestHandler handler, TResponse response, RequestMetadata metadata, CancellationToken ct)
    {
        var span = metadata.PostPipeline.AsSpan();
        for (var i = 0; i < span.Length; i++)
        {
            var group = span[i].AsSpan();
            var tasks = new Task[group.Length];
            for (var j = 0; j < group.Length; j++)
            {
                var pipelineMetadata = group[j];
                var pipeline = (IRequestHandlerPostPipeline)provider.GetRequiredService(pipelineMetadata.InterfaceType);

                pipelineMetadata.ImplementType ??= pipeline.GetType();
                var method = InvokerCache.GetPostPipeline<TResponse>(requestType, metadata, pipelineMetadata);

                tasks[j] = method(pipeline, request, handler, response, ct);
            }
            Task.WaitAll(tasks, ct);
        }
    }
    #endregion
}
