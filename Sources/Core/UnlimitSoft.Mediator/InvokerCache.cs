using FluentValidation;
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
#if NET9_0_OR_GREATER
    internal static readonly Lock Sync = new();                    // Object used as a monitor for threads synchronization.
#else
    internal static readonly object Sync = new();                  // Object used as a monitor for threads synchronization.
#endif

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
            Interlocked.CompareExchange(ref _cache, [], null);
            return _cache;
        }
    }
    internal static void UnsafeAdd(Type requestType, RequestMetadata metadata)
    {
        Cache.Add(requestType, metadata);
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
        lock (metadata.Sync)
        {
            var cli = metadata.HandlerCLI;
            if (cli is not null)
                return (Func<IRequestHandler, IRequest<TResponse>, CancellationToken, ValueTask<TResponse>>)cli;                         // Return function

            var method = metadata
                .HandlerImplementType
                .GetMethod(HandleMethod, [requestType, typeof(CancellationToken)]) ?? throw new MissingMethodException($"Can't find {HandleMethod} in {requestType}");

            var name = $"{HandleMethod}_{requestType.FullName}";

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
        }
    }
    /// <summary>
    /// Get wrapper function to invoque the <see cref="IRequestHandlerValidator{TRequest}.ValidatorAsync(TRequest, Validation.RequestValidator{TRequest}, CancellationToken)" /> asociate to the request.
    /// </summary>
    /// <param name="requestType"></param>
    /// <param name="metadata"></param>
    /// <returns></returns>
    /// <exception cref="MissingMethodException"></exception>
    internal static Func<IRequestHandler, IRequest, IValidator, CancellationToken, ValueTask<IResponse>> GetValidator(Type requestType, RequestMetadata metadata)
    {
        var cli = metadata.ValidatorCLI;
        if (cli is not null)
            return cli;

        lock (metadata.Sync)
        {
            cli = metadata.ValidatorCLI;
            if (cli is not null)
                return cli;                         // Return function

            var validatorType = metadata.Validator!;

            var method = metadata
                .HandlerImplementType
                .GetMethod(ValidatorMethod, [requestType, validatorType, typeof(CancellationToken)]) ?? throw new MissingMethodException($"Can't find {ValidatorMethod} in {requestType}");

            var name = $"{ValidatorMethod}_{requestType.FullName}";

            var returnType = typeof(ValueTask<IResponse>);
            var parameterTypes = new[] { typeof(IRequestHandler), typeof(IRequest), typeof(IValidator), typeof(CancellationToken) };
            var dynamicMethod = new DynamicMethod(name, returnType, parameterTypes, typeof(ServiceProviderMediator).Module);    // Create a dynamic method

            var il = dynamicMethod.GetILGenerator();                        // Get a ILGenerator to emit the method body

            il.Emit(OpCodes.Ldarg_0);                                       // Load the first argument (IRequestHandler) onto the evaluation stack
            il.Emit(OpCodes.Castclass, metadata.HandlerImplementType);      // Cast the IRequestHandler to handlerType (assuming handlerType is the specific type)
            il.Emit(OpCodes.Ldarg_1);                                       // Load the second argument (IRequest<TResponse>) onto the evaluation stack
            il.Emit(OpCodes.Castclass, requestType);                        // Cast the IRequest<TResponse> to requestType (assuming SomeType is the specific type)
            il.Emit(OpCodes.Ldarg_2);                                       // Load the third  argument (IValidator) onto the evaluation stack
            il.Emit(OpCodes.Castclass, validatorType);                      // Cast the IValidator to validatorType (assuming SomeType is the specific type)
            il.Emit(OpCodes.Ldarg_3);                                       // Load the third  argument (CancellationToken onto the evaluation stack
            il.Emit(OpCodes.Call, method);                                  // Call handler
            il.Emit(OpCodes.Ret);                                           // Return the result

            // Create a delegate from the dynamic method
            var tmp = dynamicMethod.CreateDelegate(typeof(Func<IRequestHandler, IRequest, IValidator, CancellationToken, ValueTask<IResponse>>));
            metadata.ValidatorCLI = (Func<IRequestHandler, IRequest, IValidator, CancellationToken, ValueTask<IResponse>>)tmp;

            return metadata.ValidatorCLI;
        }
    }
    /// <summary>
    /// Get wrapper function to invoque the <see cref="IRequestHandlerCompliance{TRequest}.ComplianceAsync(TRequest, CancellationToken)"/> asociate to the request.
    /// </summary>
    /// <param name="requestType"></param>
    /// <param name="metadata"></param>
    /// <returns></returns>
    /// <exception cref="MissingMethodException"></exception>
    internal static Func<IRequestHandler, IRequest, CancellationToken, ValueTask<IResponse>> GetCompliance(Type requestType, RequestMetadata metadata)
    {
        var cli = metadata.ComplianceCLI;
        if (cli is not null)
            return cli;

        lock (metadata.Sync)
        {
            cli = metadata.ComplianceCLI;
            if (cli is not null)
                return cli;                         // Return function

            var method = metadata
                .HandlerImplementType
                .GetMethod(ComplianceMethod, [requestType, typeof(CancellationToken)]) ?? throw new MissingMethodException($"Can't find {ComplianceMethod} in {requestType}");

            var name = $"{ComplianceMethod}_{requestType.FullName}";

            var returnType = typeof(ValueTask<IResponse>);
            var parameterTypes = new[] { typeof(IRequestHandler), typeof(IRequest), typeof(CancellationToken) };
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
            cli = (Func<IRequestHandler, IRequest, CancellationToken, ValueTask<IResponse>>)dynamicMethod.CreateDelegate(
                typeof(Func<IRequestHandler, IRequest, CancellationToken, ValueTask<IResponse>>)
            );
            metadata.ComplianceCLI = cli;
            return cli;
        }
    }
    /// <summary>
    /// Get wrapper function to invoque the <see cref="IRequestHandlerLifeCycle{TRequest}.InitAsync(TRequest, CancellationToken)"/> asociate to the request.
    /// </summary>
    /// <param name="requestType"></param>
    /// <param name="metadata"></param>
    /// <returns></returns>
    /// <exception cref="MissingMethodException"></exception>
    internal static Func<IRequestHandler, IRequest, CancellationToken, ValueTask> GetInit(Type requestType, RequestMetadata metadata)
    {
        var cli = metadata.InitCLI;
        if (cli is not null)
            return cli;

        lock (metadata.Sync)
        {
            cli = metadata.InitCLI;
            if (cli is not null)
                return cli;

            var method = metadata
                .HandlerImplementType
                .GetMethod(InitMethod, [requestType, typeof(CancellationToken)]) ?? throw new MissingMethodException($"Can't find {InitMethod} in {requestType}");

            var name = $"{InitMethod}_{requestType.FullName}";

            var returnType = typeof(ValueTask);
            var parameterTypes = new[] { typeof(IRequestHandler), typeof(IRequest), typeof(CancellationToken) };
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
            cli = (Func<IRequestHandler, IRequest, CancellationToken, ValueTask>)dynamicMethod.CreateDelegate(
                typeof(Func<IRequestHandler, IRequest, CancellationToken, ValueTask>)
            );
            metadata.InitCLI = cli;
            return cli;
        }
    }
    /// <summary>
    /// Get wrapper function to invoque the <see cref="IRequestHandlerLifeCycle{TRequest}.EndAsync(TRequest, CancellationToken)"/> asociate to the request.
    /// </summary>
    /// <param name="requestType"></param>
    /// <param name="metadata"></param>
    /// <returns></returns>
    /// <exception cref="MissingMethodException"></exception>
    internal static Func<IRequestHandler, IRequest, CancellationToken, ValueTask> GetEnd(Type requestType, RequestMetadata metadata)
    {
        var cli = metadata.EndCLI;
        if (cli is not null)
            return cli;

        lock (metadata.Sync)
        {
            cli = metadata.EndCLI;
            if (cli is not null)
                return cli;

            var method = metadata
                .HandlerImplementType
                .GetMethod(EndMethod, [requestType, typeof(CancellationToken)]) ?? throw new MissingMethodException($"Can't find {EndMethod} in {requestType}");

            var name = $"{EndMethod}_{requestType.FullName}";

            var returnType = typeof(ValueTask);
            var parameterTypes = new[] { typeof(IRequestHandler), typeof(IRequest), typeof(CancellationToken) };
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
            cli = (Func<IRequestHandler, IRequest, CancellationToken, ValueTask>)dynamicMethod.CreateDelegate(
                typeof(Func<IRequestHandler, IRequest, CancellationToken, ValueTask>)
            );
            metadata.EndCLI = cli;
            return cli;
        }
    }
    ///// <summary>
    ///// Get a function to execute the commmand handler validator without use a dynamic methods (faster)
    ///// </summary>
    ///// <param name="requestType"></param>
    ///// <param name="metadata"></param>
    ///// <param name="postPipelineMetadata"></param>
    ///// <returns></returns>
    ///// <exception cref="KeyNotFoundException"></exception>
    //internal static Func<IRequestHandlerPostPipeline, IRequest, IRequestHandler, TResponse, CancellationToken, Task> GetPostPipeline<TResponse>(Type requestType, RequestMetadata metadata, PostPipelineMetadata postPipelineMetadata)
    //{
    //    var cli = postPipelineMetadata.CLI;
    //    if (cli is not null)
    //        return (Func<IRequestHandlerPostPipeline, IRequest, IRequestHandler, TResponse, CancellationToken, Task>)cli;

    //    lock (metadata.Sync)
    //    {
    //        cli = postPipelineMetadata.CLI;
    //        if (cli is null)
    //        {
    //            var handlerType = metadata.HandlerImplementType;
    //            var postPipelineType = postPipelineMetadata.ImplementType!;

    //            var method = postPipelineType
    //                .GetMethod(PostPipelineMethod, new[] { requestType, handlerType, typeof(TResponse), typeof(CancellationToken) }) ?? throw new MissingMethodException($"Can't find {PostPipelineMethod} in {requestType}");

    //            var tmp = Emit<Func<IRequestHandlerPostPipeline, IRequest, IRequestHandler, TResponse, CancellationToken, Task>>
    //                .NewDynamicMethod($"{PostPipelineMethod}_{handlerType.FullName}")
    //                .LoadArgument(0).CastClass(postPipelineType)
    //                .LoadArgument(1).CastClass(requestType)
    //                .LoadArgument(2).CastClass(handlerType)
    //                .LoadArgument(3)
    //                .LoadArgument(4)
    //                .Call(method)
    //                .Return()
    //                .CreateDelegate();
    //            postPipelineMetadata.CLI = tmp;

    //            return tmp;
    //        }
    //    }

    //    // Return function
    //    return (Func<IRequestHandlerPostPipeline, IRequest, IRequestHandler, TResponse, CancellationToken, Task>)cli;
    //}
}
