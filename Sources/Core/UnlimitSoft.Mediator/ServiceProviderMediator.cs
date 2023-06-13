using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sigil;
using System;
using System.Collections.Generic;
using System.Linq;
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

    private static Dictionary<Type, RequestMetadata>? _cache;
    private const string 
        InitMethod = nameof(IRequestHandlerLifeCycle<IRequest>.InitAsync), 
        HandleMethod = nameof(IRequestHandler<IRequest<object>, object>.HandleAsync),
        ValidatorMethod = "ValidatorAsync", 
        ComplianceMethod = "ComplianceAsync", 
        PostPipelineMethod = "HandleAsync", 
        EndMethod = nameof(IRequestHandlerLifeCycle<IRequest>.EndAsync);


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
    public async ValueTask<Result<TResponse>> SendAsync<TResponse>(IServiceProvider provider, IRequest<TResponse> request, CancellationToken ct = default)
    {
        _logger?.LogDebug(10, "Process request: {@Request}", request);

        //
        // Get handler and execute command.
        var requestType = request.GetType();
        var responseType = typeof(TResponse);
        _logger?.LogDebug(12, "Execute request type {Request} with response {Response}", requestType, responseType);

        var handler = BuildHandlerMetadata(provider, requestType, responseType, out var metadata);
        if (handler is null)
            return new Result<TResponse>(default, request.NotFoundResponse());

        if (metadata.HasLifeCycle)
            await InitAsync(handler, request, requestType, metadata, ct);

        if (_validate)
        {
            if (metadata.Validator is not null)
            {
                var error = await ValidationAsync(handler, request, requestType, metadata, ct);
                if (error is not null)
                    return new Result<TResponse>(default, error);
            }
            if (metadata.HasCompliance)
            {
                var error = await ComplianceAsync(handler, request, requestType, metadata, ct);
                if (error is not null)
                    return new Result<TResponse>(default, error);
            }
        }
        var response = await HandlerAsync(handler, request, requestType, metadata, ct);

        // Run existing post operations
        if (metadata.PostPipeline is not null)
            PostPipelineHandlerAsync(provider, requestType, request, handler, response, metadata, ct);

        if (metadata.HasLifeCycle)
            await EndAsync(handler, request, requestType, metadata, ct);

        return new Result<TResponse>(response, null);
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
    /// <summary>
    /// Return an instance of the cache object. Cache is share between all instance of the Mediator to avoid wasted memory and processing.
    /// </summary>
    private static Dictionary<Type, RequestMetadata> Cache
    {
        get
        {
            if (_cache is not null)
                return _cache;
            Interlocked.CompareExchange(ref _cache, new Dictionary<Type, RequestMetadata>(), null);
            return _cache;
        }
    }

    private static IRequestHandler? BuildHandlerMetadata(IServiceProvider provider, Type requestType, Type responseType, out RequestMetadata metadata)
    {
        if (Cache.TryGetValue(requestType, out metadata!))
        {
            if (metadata.HandlerInterfaceType is not null)
                return (IRequestHandler)provider.GetRequiredService(metadata.HandlerInterfaceType);
            return null;
        }

        // Handler
        lock (Cache)
            if (!Cache.TryGetValue(requestType, out metadata!))
            {
                // Create Handler
                var handlerInterfaceType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);
                var handler = (IRequestHandler)provider.GetService(handlerInterfaceType);
                if (handler is null)
                {
                    Cache.Add(requestType, new RequestMetadata());
                    return null;
                }

                // Features implemented by this command
                var handleType = handler.GetType();
                metadata = new RequestMetadata
                {
                    HandlerInterfaceType = handlerInterfaceType,
                    HandlerImplementType = handleType
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

                Cache.Add(requestType, metadata);
                return handler;
            }

        // Resolve service
        return (IRequestHandler)provider.GetRequiredService(metadata.HandlerInterfaceType);
    }

    private static Func<IRequestHandler, IRequest<TResponse>, CancellationToken, ValueTask<TResponse>> GetHandler<TResponse>(Type requestType, RequestMetadata metadata)
    {
        var cli = metadata.HandlerCLI;
        if (cli is not null)
            return (Func<IRequestHandler, IRequest<TResponse>, CancellationToken, ValueTask<TResponse>>)cli;

        lock (metadata)
        {
            cli = metadata.HandlerCLI;
            if (cli is null)
            {
                var method = metadata
                    .HandlerImplementType
                    .GetMethod(HandleMethod, new Type[] { requestType, typeof(CancellationToken) });
                if (method is null)
                    throw new MissingMethodException($"Can't find {HandleMethod} in {requestType}");

                var tmp = Emit<Func<IRequestHandler, IRequest<TResponse>, CancellationToken, ValueTask<TResponse>>>
                    .NewDynamicMethod($"{HandleMethod}_{requestType.FullName}")
                    .LoadArgument(0).CastClass(metadata.HandlerImplementType)
                    .LoadArgument(1).CastClass(requestType)
                    .LoadArgument(2)
                    .Call(method)
                    .Return()
                    .CreateDelegate();

                metadata.HandlerCLI = tmp;
                return tmp;
            }
        }

        // Return function
        return (Func<IRequestHandler, IRequest<TResponse>, CancellationToken, ValueTask<TResponse>>)cli;
    }
    private static async ValueTask<TResponse?> HandlerAsync<TResponse>(IRequestHandler handler, IRequest<TResponse> request, Type requestType, RequestMetadata metadata, CancellationToken ct)
    {
        var method = GetHandler<TResponse>(requestType, metadata);
        return await method(handler, request, ct);
    }

    private static Func<IRequestHandler, IRequest, IValidator, CancellationToken, ValueTask<IResponse>> GetValidator(Type requestType, RequestMetadata metadata)
    {
        var cli = metadata.ValidatorCLI;
        if (cli is not null)
            return cli;

        lock (metadata)
        {
            cli = metadata.ValidatorCLI;
            if (cli is null)
            {
                var validatorType = metadata.Validator!;

                var method = metadata
                    .HandlerImplementType
                    .GetMethod(ValidatorMethod, new Type[] { requestType, validatorType, typeof(CancellationToken) });
                if (method is null)
                    throw new MissingMethodException($"Can't find {ValidatorMethod} in {requestType}");

                var tmp = Emit<Func<IRequestHandler, IRequest, IValidator, CancellationToken, ValueTask<IResponse>>>
                    .NewDynamicMethod($"{ValidatorMethod}_{requestType.FullName}")
                    .LoadArgument(0).CastClass(metadata.HandlerImplementType)
                    .LoadArgument(1).CastClass(requestType)
                    .LoadArgument(2).CastClass(validatorType)
                    .LoadArgument(3)
                    .Call(method)
                    .Return()
                    .CreateDelegate();

                metadata.ValidatorCLI = tmp;
                return tmp;
            }
        }

        // Return function
        return cli;
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
        var method = GetValidator(requestType, metadata);

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

    private static Func<IRequestHandler, IRequest, CancellationToken, ValueTask<IResponse>> GetCompliance(Type requestType, RequestMetadata metadata)
    {
        var cli = metadata.ComplianceCLI;
        if (cli is not null)
            return cli;

        lock (metadata)
        {
            cli = metadata.ComplianceCLI;
            if (cli is null)
            {
                var method = metadata
                    .HandlerImplementType
                    .GetMethod(ComplianceMethod, new Type[] { requestType, typeof(CancellationToken) });
                if (method is null)
                    throw new MissingMethodException($"Can't find {ComplianceMethod} in {requestType}");

                var tmp = Emit<Func<IRequestHandler, IRequest, CancellationToken, ValueTask<IResponse>>>
                    .NewDynamicMethod($"{ComplianceMethod}_{requestType.FullName}")
                    .LoadArgument(0).CastClass(metadata.HandlerImplementType)
                    .LoadArgument(1).CastClass(requestType)
                    .LoadArgument(2)
                    .Call(method)
                    .Return()
                    .CreateDelegate();

                metadata.ComplianceCLI = tmp;
                return tmp;
            }
        }

        // Return function
        return cli;
    }
    private async ValueTask<IResponse?> ComplianceAsync(IRequestHandler handler, IRequest request, Type requestType, RequestMetadata metadata, CancellationToken ct)
    {
        _logger?.LogDebug("Request {Request} handler implement internal compliance", request);

        var method = GetCompliance(requestType, metadata);
        var response = await method(handler, request, ct);
        if (!response.IsSuccess)
            return response;

        return null;
    }

    private static Func<IRequestHandler, IRequest, CancellationToken, ValueTask> GetInit(Type requestType, RequestMetadata metadata)
    {
        var cli = metadata.InitCLI;
        if (cli is not null)
            return cli;

        lock (metadata)
        {
            cli = metadata.InitCLI;
            if (cli is not null)
                return cli;

            var method = metadata
                .HandlerImplementType
                .GetMethod(InitMethod, new Type[] { requestType, typeof(CancellationToken) })
                ?? throw new MissingMethodException($"Can't find {InitMethod} in {requestType}");

            cli = Emit<Func<IRequestHandler, IRequest, CancellationToken, ValueTask>>
                .NewDynamicMethod($"{InitMethod}_{requestType.FullName}")
                .LoadArgument(0).CastClass(metadata.HandlerImplementType)
                .LoadArgument(1).CastClass(requestType)
                .LoadArgument(2)
                .Call(method)
                .Return()
                .CreateDelegate();

            metadata.InitCLI = cli;
        }

        // Return function
        return cli;
    }
    private ValueTask InitAsync<TResponse>(IRequestHandler handler, IRequest<TResponse> request, Type requestType, RequestMetadata metadata, CancellationToken ct)
    {
        _logger?.LogDebug("Request {Request} handler implement internal life cycle", request);

        var method = GetInit(requestType, metadata);
        return method(handler, request, ct);
    }

    private static Func<IRequestHandler, IRequest, CancellationToken, ValueTask> GetEnd(Type requestType, RequestMetadata metadata)
    {
        var cli = metadata.EndCLI;
        if (cli is not null)
            return cli;

        lock (metadata)
        {
            cli = metadata.EndCLI;
            if (cli is not null)
                return cli;

            var method = metadata
                .HandlerImplementType
                .GetMethod(EndMethod, new Type[] { requestType, typeof(CancellationToken) })
                ?? throw new MissingMethodException($"Can't find {EndMethod} in {requestType}");

            cli = Emit<Func<IRequestHandler, IRequest, CancellationToken, ValueTask>>
                .NewDynamicMethod($"{EndMethod}_{requestType.FullName}")
                .LoadArgument(0).CastClass(metadata.HandlerImplementType)
                .LoadArgument(1).CastClass(requestType)
                .LoadArgument(2)
                .Call(method)
                .Return()
                .CreateDelegate();

            metadata.EndCLI = cli;
        }

        // Return function
        return cli;
    }
    private ValueTask EndAsync<TResponse>(IRequestHandler handler, IRequest<TResponse> request, Type requestType, RequestMetadata metadata, CancellationToken ct)
    {
        _logger?.LogDebug("Request {Request} handler implement internal life cycle", request);

        var method = GetEnd(requestType, metadata);
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
                var method = ServiceProviderMediator.GetPostPipeline<TResponse>(requestType, metadata, pipelineMetadata);

                tasks[j] = method(pipeline, request, handler, response, ct);
            }
            Task.WaitAll(tasks, ct);
        }
    }
    /// <summary>
    /// Get a function to execute the commmand handler validator without use a dynamic methods (faster)
    /// </summary>
    /// <param name="requestType"></param>
    /// <param name="metadata"></param>
    /// <param name="postPipelineMetadata"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    private static Func<IRequestHandlerPostPipeline, IRequest, IRequestHandler, TResponse, CancellationToken, Task> GetPostPipeline<TResponse>(Type requestType, RequestMetadata metadata, PostPipelineMetadata postPipelineMetadata)
    {
        var cli = postPipelineMetadata.CLI;
        if (cli is not null)
            return (Func<IRequestHandlerPostPipeline, IRequest, IRequestHandler, TResponse, CancellationToken, Task>)cli;

        lock (metadata)
        {
            cli = postPipelineMetadata.CLI;
            if (cli is null)
            {
                var handlerType = metadata.HandlerImplementType;
                var postPipelineType = postPipelineMetadata.ImplementType!;

                var method = postPipelineType
                    .GetMethod(PostPipelineMethod, new Type[] { requestType, handlerType, typeof(TResponse), typeof(CancellationToken) });

                var tmp = Emit<Func<IRequestHandlerPostPipeline, IRequest, IRequestHandler, TResponse, CancellationToken, Task>>
                    .NewDynamicMethod($"{PostPipelineMethod}_{handlerType.FullName}")
                    .LoadArgument(0).CastClass(postPipelineType)
                    .LoadArgument(1).CastClass(requestType)
                    .LoadArgument(2).CastClass(handlerType)
                    .LoadArgument(3)
                    .LoadArgument(4)
                    .Call(method)
                    .Return()
                    .CreateDelegate();
                postPipelineMetadata.CLI = tmp;

                return tmp;
            }
        }

        // Return function
        return (Func<IRequestHandlerPostPipeline, IRequest, IRequestHandler, TResponse, CancellationToken, Task>)cli;
    }
    #endregion

    #region Nested Classes
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private sealed class RequestMetadata
    {
        public Type? Validator;
        public Func<IRequestHandler, IRequest, IValidator, CancellationToken, ValueTask<IResponse>>? ValidatorCLI;

        public Type HandlerInterfaceType;
        public Type HandlerImplementType;
        public object? HandlerCLI;

        public bool HasCompliance;
        public Func<IRequestHandler, IRequest, CancellationToken, ValueTask<IResponse>>? ComplianceCLI;

        public bool HasLifeCycle;
        public Func<IRequestHandler, IRequest, CancellationToken, ValueTask>? InitCLI, EndCLI;

        public PostPipelineMetadata[][]? PostPipeline;
    }
    private sealed class PostPipelineMetadata
    {
        public Type InterfaceType;
        public Type? ImplementType;
        public object? CLI;
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    #endregion
}
