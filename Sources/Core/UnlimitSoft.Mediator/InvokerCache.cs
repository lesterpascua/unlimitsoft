using FluentValidation;
using Sigil;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.Mediator.Pipeline;
using UnlimitSoft.Message;

namespace UnlimitSoft.Mediator;


/// <summary>
/// Storage the diferent method used in the livecicle of the <see cref="IMediator"/> to math <see cref="IRequest"/> and <see cref="IRequestHandler"/>.
/// </summary>
public static class InvokerCache
{
    private static Dictionary<Type, RequestMetadata>? _cache;

    internal const string
        InitMethod = nameof(IRequestHandlerLifeCycle<IRequest>.InitAsync),
        HandleMethod = nameof(IRequestHandler<IRequest<object>, object>.HandleAsync),
        ValidatorMethod = nameof(IRequestHandlerValidator<IRequest<object>>.ValidatorAsync),
        ComplianceMethod = nameof(IRequestHandlerCompliance<IRequest<object>>.ComplianceAsync),
        PostPipelineMethod = nameof(IRequestHandlerPostPipeline<IRequest<object>, IRequestHandler, object, IRequestHandlerPostPipeline>.HandleAsync),
        EndMethod = nameof(IRequestHandlerLifeCycle<IRequest>.EndAsync);



    /// <summary>
    /// Return an instance of the cache object. Cache is share between all instance of the Mediator to avoid wasted memory and processing.
    /// </summary>
    internal static Dictionary<Type, RequestMetadata> Cache
    {
        get
        {
            if (_cache is not null)
                return _cache;
            Interlocked.CompareExchange(ref _cache, new Dictionary<Type, RequestMetadata>(), null);
            return _cache;
        }
    }

    /// <summary>
    /// Get wrapper function to invoque the <see cref="IRequestHandler{TRequest, TResponse}.HandleAsync(TRequest, CancellationToken)"/> asociate to the request.
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="requestType"></param>
    /// <param name="metadata"></param>
    /// <returns></returns>
    /// <exception cref="MissingMethodException"></exception>
    internal static Func<IRequestHandler, IRequest<TResponse>, CancellationToken, ValueTask<TResponse>> GetHandler<TResponse>(Type requestType, RequestMetadata metadata)
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
                    .GetMethod(HandleMethod, new[] { requestType, typeof(CancellationToken) }) ?? throw new MissingMethodException($"Can't find {HandleMethod} in {requestType}");

                var name = $"{HandleMethod}_{requestType.FullName}";

#if EMIT_NATIVE
                var returnType = typeof(ValueTask<TResponse>);
                var parameterTypes = new[] { typeof(IRequestHandler), typeof(IRequest<TResponse>), typeof(CancellationToken) };
                var dynamicMethod = new DynamicMethod(name, returnType, parameterTypes, typeof(ServiceProviderMediator).Module);    // Create a dynamic method

                var il = dynamicMethod.GetILGenerator();                        // Get a ILGenerator to emit the method body

                il.Emit(OpCodes.Ldarg_0);                                       // Load the first argument (IRequestHandler) onto the evaluation stack
                il.Emit(OpCodes.Castclass, metadata.HandlerImplementType);      // Cast the IRequestHandler to handlerType (assuming handlerType is the specific type)
                il.Emit(OpCodes.Ldarg_1);                                       // Load the second argument (IRequest<TResponse>) onto the evaluation stack
                il.Emit(OpCodes.Castclass, requestType);                        // Cast the IRequest<TResponse> to requestType (assuming SomeType is the specific type)
                il.Emit(OpCodes.Ldarg_2);                                       // Load the third  argument (CancellationToken onto the evaluation stack
                il.Emit(OpCodes.Call, method);                                  // Call handler
                il.Emit(OpCodes.Ret);                                           // Return the result

                // Create a delegate from the dynamic method
                var tmp = dynamicMethod.CreateDelegate(typeof(Func<IRequestHandler, IRequest<TResponse>, CancellationToken, ValueTask<TResponse>>));
                metadata.HandlerCLI = tmp;

                return (Func<IRequestHandler, IRequest<TResponse>, CancellationToken, ValueTask<TResponse>>)tmp;
#else
                var tmp = Emit<Func<IRequestHandler, IRequest<TResponse>, CancellationToken, ValueTask<TResponse>>>
                    .NewDynamicMethod(name)
                    .LoadArgument(0).CastClass(metadata.HandlerImplementType)
                    .LoadArgument(1).CastClass(requestType)
                    .LoadArgument(2)
                    .Call(method)
                    .Return()
                    .CreateDelegate();

                metadata.HandlerCLI = tmp;
                return tmp;
#endif
            }
        }

        // Return function
        return (Func<IRequestHandler, IRequest<TResponse>, CancellationToken, ValueTask<TResponse>>)cli;
    }

    internal static Func<IRequestHandler, IRequest, IValidator, CancellationToken, ValueTask<IResponse>> GetValidator(Type requestType, RequestMetadata metadata)
    {
        var cli = metadata.ValidatorCLI;
        if (cli is not null)
            return cli;

        lock (metadata)
        {
            cli = metadata.ValidatorCLI;
            if (cli is not null)
                return cli;                         // Return function

            var validatorType = metadata.Validator!;

            var method = metadata
                .HandlerImplementType
                .GetMethod(ValidatorMethod, new[] { requestType, validatorType, typeof(CancellationToken) }) ?? throw new MissingMethodException($"Can't find {ValidatorMethod} in {requestType}");

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
    internal static Func<IRequestHandler, IRequest, CancellationToken, ValueTask<IResponse>> GetCompliance(Type requestType, RequestMetadata metadata)
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
                    .GetMethod(ComplianceMethod, new[] { requestType, typeof(CancellationToken) }) ?? throw new MissingMethodException($"Can't find {ComplianceMethod} in {requestType}");
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
    internal static Func<IRequestHandler, IRequest, CancellationToken, ValueTask> GetInit(Type requestType, RequestMetadata metadata)
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
                .GetMethod(InitMethod, new[] { requestType, typeof(CancellationToken) }) ?? throw new MissingMethodException($"Can't find {InitMethod} in {requestType}");

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
    internal static Func<IRequestHandler, IRequest, CancellationToken, ValueTask> GetEnd(Type requestType, RequestMetadata metadata)
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
                .GetMethod(EndMethod, new[] { requestType, typeof(CancellationToken) }) ?? throw new MissingMethodException($"Can't find {EndMethod} in {requestType}");

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
    /// <summary>
    /// Get a function to execute the commmand handler validator without use a dynamic methods (faster)
    /// </summary>
    /// <param name="requestType"></param>
    /// <param name="metadata"></param>
    /// <param name="postPipelineMetadata"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    internal static Func<IRequestHandlerPostPipeline, IRequest, IRequestHandler, TResponse, CancellationToken, Task> GetPostPipeline<TResponse>(Type requestType, RequestMetadata metadata, PostPipelineMetadata postPipelineMetadata)
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
                    .GetMethod(PostPipelineMethod, new[] { requestType, handlerType, typeof(TResponse), typeof(CancellationToken) }) ?? throw new MissingMethodException($"Can't find {PostPipelineMethod} in {requestType}");

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
}
