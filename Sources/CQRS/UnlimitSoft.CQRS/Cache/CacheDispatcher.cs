﻿using FluentValidation;
using Sigil;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.Command;
using UnlimitSoft.CQRS.Command.Pipeline;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.CQRS.Message;
using UnlimitSoft.CQRS.Query;
using UnlimitSoft.Event;
using UnlimitSoft.Message;

namespace UnlimitSoft.CQRS.Cache;


/// <summary>
/// Store precompiler call for Query, Command and Events
/// </summary>
internal static class CacheDispatcher
{
    private const string HandleAsync = "HandleAsync";
    private const string ValidatorAsync = "ValidatorAsync";
    private const string ComplianceAsync = "ComplianceAsync";
    private const string PostPipelineAsync = "HandleAsync";



    #region Commands
    private static Dictionary<Type, CommandMethod>? _commandCache;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Dictionary<Type, CommandMethod> GetCommandMeta()
    {
        if (_commandCache is not null)
            return _commandCache;

        Interlocked.CompareExchange(ref _commandCache, new(), null);
        return _commandCache;
    }

    /// <summary>
    /// Get a function to execute the commmand handler without use a dynamic methods (faster)
    /// </summary>
    /// <param name="commandType"></param>
    /// <param name="handler"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public static Func<ICommandHandler, ICommand, CancellationToken, ValueTask<IResponse>> GetCommandHandler(Type commandType, ICommandHandler handler)
    {
        var cache = GetCommandMeta();
        if (cache.TryGetValue(commandType, out var metadata) && metadata.Handler is not null)
            return metadata.Handler;

        lock (cache)
        {
            if (!cache.TryGetValue(commandType, out metadata))
                cache.Add(commandType, metadata = new CommandMethod());
            if (metadata.Handler is not null)
                return metadata.Handler;

            var method = handler
                .GetType()
                .GetMethod(HandleAsync, new Type[] { commandType, typeof(CancellationToken) });
            if (method is null)
                throw new KeyNotFoundException($"Not found handler for {handler}");

            var handlerType = handler.GetType();
            metadata.Handler = Emit<Func<ICommandHandler, ICommand, CancellationToken, ValueTask<IResponse>>>
                .NewDynamicMethod($"{HandleAsync}_{commandType.FullName}")
                .LoadArgument(0).CastClass(handlerType)
                .LoadArgument(1).CastClass(commandType)
                .LoadArgument(2)
                .Call(method)
                .Return()
                .CreateDelegate();

            return metadata.Handler;
        }
    }
    /// <summary>
    /// Get a function to execute the commmand handler compliance without use a dynamic methods (faster)
    /// </summary>
    /// <param name="commandType"></param>
    /// <param name="handler"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public static Func<ICommandHandler, ICommand, CancellationToken, ValueTask<IResponse>> GetCommandCompliance(Type commandType, ICommandHandler handler)
    {
        var cache = GetCommandMeta();
        if (cache.TryGetValue(commandType, out var metadata) && metadata.Compliance is not null)
            return metadata.Compliance;

        lock (cache)
        {
            if (!cache.TryGetValue(commandType, out metadata))
                cache.Add(commandType, metadata = new CommandMethod());
            if (metadata.Compliance is not null)
                return metadata.Compliance;

            var method = handler
                .GetType()
                .GetMethod(ComplianceAsync, new Type[] { commandType, typeof(CancellationToken) });
            if (method is null)
                throw new KeyNotFoundException($"Not found validator for {handler}");

            var handlerType = handler.GetType();
            metadata.Compliance = Emit<Func<ICommandHandler, ICommand, CancellationToken, ValueTask<IResponse>>>
                .NewDynamicMethod($"{ComplianceAsync}_{commandType.FullName}")
                .LoadArgument(0).CastClass(handlerType)
                .LoadArgument(1).CastClass(commandType)
                .LoadArgument(2)
                .Call(method)
                .Return()
                .CreateDelegate();

            return metadata.Compliance;
        }
    }
    /// <summary>
    /// Get a function to execute the commmand handler validator without use a dynamic methods (faster)
    /// </summary>
    /// <param name="commandType"></param>
    /// <param name="validatorType"></param>
    /// <param name="handler"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public static Func<ICommandHandler, ICommand, IValidator, CancellationToken, ValueTask<IResponse>> GetCommandValidator(Type commandType, Type validatorType, ICommandHandler handler)
    {
        var cache = GetCommandMeta();
        if (cache.TryGetValue(commandType, out var metadata) && metadata.Validator is not null)
            return metadata.Validator;

        lock (cache)
        {
            if (!cache.TryGetValue(commandType, out metadata))
                cache.Add(commandType, metadata = new CommandMethod());
            if (metadata.Validator is not null)
                return metadata.Validator;

            var method = handler
                .GetType()
                .GetMethod(ValidatorAsync, new Type[] { commandType, validatorType, typeof(CancellationToken) });
            if (method is null)
                throw new KeyNotFoundException($"Not found validator for {handler}");

            var handlerType = handler.GetType();
            metadata.Validator = Emit<Func<ICommandHandler, ICommand, IValidator, CancellationToken, ValueTask<IResponse>>>
                .NewDynamicMethod($"{ValidatorAsync}_{commandType.FullName}")
                .LoadArgument(0).CastClass(handlerType)
                .LoadArgument(1).CastClass(commandType)
                .LoadArgument(2).CastClass(validatorType)
                .LoadArgument(3)
                .Call(method)
                .Return()
                .CreateDelegate();

            return metadata.Validator;
        }
    }

    /// <summary>
    /// Get a function to execute the commmand handler validator without use a dynamic methods (faster)
    /// </summary>
    /// <param name="commandType"></param>
    /// <param name="commandHandlerType"></param>
    /// <param name="pipelineHandler"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public static Func<ICommandHandlerPostPipeline, ICommand, ICommandHandler, IResponse, CancellationToken, Task> GetCommandPostPipeline(Type commandType, Type commandHandlerType, ICommandHandlerPostPipeline pipelineHandler)
    {
        Type pipelineHandleType = pipelineHandler.GetType();

        var cache = GetCommandMeta();
        if (cache.TryGetValue(commandType, out var metadata) && metadata.PostPipelines is not null && metadata.PostPipelines.TryGetValue(pipelineHandleType, out var handle))
            return handle;

        lock (cache)
        {
            if (!cache.TryGetValue(commandType, out metadata))
                cache.Add(commandType, metadata = new CommandMethod());

            metadata.PostPipelines ??= new();
            if (metadata.PostPipelines.TryGetValue(pipelineHandleType, out handle))
                return handle;

            var method = pipelineHandler
                .GetType()
                .GetMethod(PostPipelineAsync, new Type[] { commandType, commandHandlerType, typeof(IResponse), typeof(CancellationToken) });
            if (method is null)
                throw new KeyNotFoundException($"Not found validator for {pipelineHandler}");

            var handlerType = pipelineHandler.GetType();
            handle = Emit<Func<ICommandHandlerPostPipeline, ICommand, ICommandHandler, IResponse, CancellationToken, Task>>
                .NewDynamicMethod($"{PostPipelineAsync}_{pipelineHandleType.FullName}")
                .LoadArgument(0).CastClass(pipelineHandleType)
                .LoadArgument(1).CastClass(commandType)
                .LoadArgument(2).CastClass(commandHandlerType)
                .LoadArgument(3)
                .LoadArgument(4)
                .Call(method)
                .Return()
                .CreateDelegate();
            metadata.PostPipelines[pipelineHandleType] = handle;

            return handle;
        }
    }

    /// <summary>
    /// Bucket cache
    /// </summary>
    private sealed class CommandMethod
    {
        public Func<ICommandHandler, ICommand, CancellationToken, ValueTask<IResponse>>? Handler;
        public Func<ICommandHandler, ICommand, IValidator, CancellationToken, ValueTask<IResponse>>? Validator;
        public Func<ICommandHandler, ICommand, CancellationToken, ValueTask<IResponse>>? Compliance;
        public Dictionary<Type, Func<ICommandHandlerPostPipeline, ICommand, ICommandHandler, IResponse, CancellationToken, Task>>? PostPipelines;
    }
    #endregion

    #region Events
    private static Dictionary<Type, Func<IEventHandler, IEvent, CancellationToken, Task<IEventResponse>>>? _eventCache;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Dictionary<Type, Func<IEventHandler, IEvent, CancellationToken, Task<IEventResponse>>> GetEventMeta()
    {
        if (_eventCache is null)
            Interlocked.CompareExchange(ref _eventCache, new(), null);
        return _eventCache;
    }

    /// <summary>
    /// Get a function to execute the commmand handler without use a dynamic methods (faster)
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="handler"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public static Func<IEventHandler, IEvent, CancellationToken, Task<IEventResponse>> GetEventHandler(Type eventType, IEventHandler handler)
    {
        var cache = GetEventMeta();
        if (cache.TryGetValue(eventType, out var metadata))
            return metadata;

        lock (cache)
        {
            if (cache.TryGetValue(eventType, out metadata))
                return metadata;

            var method = handler
                .GetType()
                .GetMethod(HandleAsync, new Type[] { eventType, typeof(CancellationToken) });
            if (method is null)
                throw new KeyNotFoundException($"Not found handler for {handler}");

            var handlerType = handler.GetType();
            var @delegate = Emit<Func<IEventHandler, IEvent, CancellationToken, Task<IEventResponse>>>
                .NewDynamicMethod($"{HandleAsync}_{eventType.FullName}")
                .LoadArgument(0).CastClass(handlerType)
                .LoadArgument(1).CastClass(eventType)
                .LoadArgument(2)
                .Call(method)
                .Return()
                .CreateDelegate();
            cache.Add(eventType, @delegate);

            return @delegate;
        }
    }
    #endregion
}
