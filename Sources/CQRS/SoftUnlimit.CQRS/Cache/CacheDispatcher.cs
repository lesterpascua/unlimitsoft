using FluentValidation;
using Sigil;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.Message;
using SoftUnlimit.CQRS.Query;
using SoftUnlimit.Event;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Cache
{
    /// <summary>
    /// Store precompiler call for Query, Command and Events
    /// </summary>
    internal static class CacheDispatcher
    {
        private const string HandleAsync = nameof(ICommandHandler<ICommand>.HandleAsync);
        private const string ValidatorAsync = nameof(ICommandHandlerValidator<ICommand>.ValidatorAsync);
        private const string ComplianceAsync = nameof(ICommandHandlerCompliance<ICommand>.ComplianceAsync);


        #region Query
        private static Dictionary<Type, QueryMethod> _queryCache;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Dictionary<Type, QueryMethod> GetQueryMeta()
        {
            if (_queryCache is null)
                Interlocked.CompareExchange(ref _queryCache, new(), null);
            return _queryCache;
        }

        /// <summary>
        /// Get a function to execute the query handler without use a dynamic methods (faster)
        /// </summary>
        /// <param name="queryType"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public static Func<IQueryHandler, IQuery, CancellationToken, Task> GetQueryHandler<TResult>(Type queryType, IQueryHandler handler)
        {
            var cache = GetQueryMeta();
            if (cache.TryGetValue(queryType, out var metadata) && metadata.Handler is not null)
                return metadata.Handler;

            lock (cache)
            {
                if (!cache.TryGetValue(queryType, out metadata))
                    cache.Add(queryType, metadata = new QueryMethod());
                if (metadata.Handler is not null)
                    return metadata.Handler;

                var method = handler
                    .GetType()
                    .GetMethod(HandleAsync, new Type[] { queryType, typeof(CancellationToken) });
                if (method is null)
                    throw new KeyNotFoundException($"Not found handler for {handler}");

                var handlerType = handler.GetType();
                metadata.Handler = Emit<Func<IQueryHandler, IQuery, CancellationToken, Task<TResult>>>
                    .NewDynamicMethod($"{HandleAsync}_{queryType.FullName}")
                    .LoadArgument(0).CastClass(handlerType)
                    .LoadArgument(1).CastClass(queryType)
                    .LoadArgument(2)
                    .Call(method)
                    .Return()
                    .CreateDelegate();

                return metadata.Handler;
            }
        }
        /// <summary>
        /// Get a function to execute the query handler compliance without use a dynamic methods (faster)
        /// </summary>
        /// <param name="queryType"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public static Func<IQueryHandler, IQuery, CancellationToken, ValueTask<IQueryResponse>> GetQueryCompliance(Type queryType, IQueryHandler handler)
        {
            var cache = GetQueryMeta();
            if (cache.TryGetValue(queryType, out var metadata) && metadata.Compliance is not null)
                return metadata.Compliance;

            lock (cache)
            {
                if (!cache.TryGetValue(queryType, out metadata))
                    cache.Add(queryType, metadata = new QueryMethod());
                if (metadata.Compliance is not null)
                    return metadata.Compliance;

                var method = handler
                    .GetType()
                    .GetMethod(ComplianceAsync, new Type[] { queryType, typeof(CancellationToken) });
                if (method is null)
                    throw new KeyNotFoundException($"Not found validator for {handler}");

                var handlerType = handler.GetType();
                metadata.Compliance = Emit<Func<IQueryHandler, IQuery, CancellationToken, ValueTask<IQueryResponse>>>
                    .NewDynamicMethod($"{ComplianceAsync}_{queryType.FullName}")
                    .LoadArgument(0).CastClass(handlerType)
                    .LoadArgument(1).CastClass(queryType)
                    .LoadArgument(2)
                    .Call(method)
                    .Return()
                    .CreateDelegate();

                return metadata.Compliance;
            }
        }
        /// <summary>
        /// Get a function to execute the query handler validator without use a dynamic methods (faster)
        /// </summary>
        /// <param name="queryType"></param>
        /// <param name="validatorType"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public static Func<IQueryHandler, IQuery, IValidator, CancellationToken, ValueTask<IQueryResponse>> GetQueryValidator(Type queryType, Type validatorType, IQueryHandler handler)
        {
            var cache = GetQueryMeta();
            if (cache.TryGetValue(queryType, out var metadata) && metadata.Validator is not null)
                return metadata.Validator;

            lock (cache)
            {
                if (!cache.TryGetValue(queryType, out metadata))
                    cache.Add(queryType, metadata = new QueryMethod());
                if (metadata.Validator is not null)
                    return metadata.Validator;

                var method = handler
                    .GetType()
                    .GetMethod(ValidatorAsync, new Type[] { queryType, validatorType, typeof(CancellationToken) });
                if (method is null)
                    throw new KeyNotFoundException($"Not found validator for {handler}");

                var handlerType = handler.GetType();
                metadata.Validator = Emit<Func<IQueryHandler, IQuery, IValidator, CancellationToken, ValueTask<IQueryResponse>>>
                    .NewDynamicMethod($"{ValidatorAsync}_{queryType.FullName}")
                    .LoadArgument(0).CastClass(handlerType)
                    .LoadArgument(1).CastClass(queryType)
                    .LoadArgument(2).CastClass(validatorType)
                    .LoadArgument(3)
                    .Call(method)
                    .Return()
                    .CreateDelegate();

                return metadata.Validator;
            }
        }

        /// <summary>
        /// Bucket cache
        /// </summary>
        private sealed class QueryMethod
        {
            public Func<IQueryHandler, IQuery, CancellationToken, Task> Handler;
            public Func<IQueryHandler, IQuery, IValidator, CancellationToken, ValueTask<IQueryResponse>> Validator;
            public Func<IQueryHandler, IQuery, CancellationToken, ValueTask<IQueryResponse>> Compliance;
        }
        #endregion

        #region Commands
        private static Dictionary<Type, CommandMethod> _commandCache;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Dictionary<Type, CommandMethod> GetCommandMeta()
        {
            if (_commandCache is null)
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
        public static Func<ICommandHandler, ICommand, CancellationToken, Task<ICommandResponse>> GetCommandHandler(Type commandType, ICommandHandler handler)
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
                metadata.Handler = Emit<Func<ICommandHandler, ICommand, CancellationToken, Task<ICommandResponse>>>
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
        public static Func<ICommandHandler, ICommand, CancellationToken, ValueTask<ICommandResponse>> GetCommandCompliance(Type commandType, ICommandHandler handler)
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
                metadata.Compliance = Emit<Func<ICommandHandler, ICommand, CancellationToken, ValueTask<ICommandResponse>>>
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
        public static Func<ICommandHandler, ICommand, IValidator, CancellationToken, ValueTask<ICommandResponse>> GetCommandValidator(Type commandType, Type validatorType, ICommandHandler handler)
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
                metadata.Validator = Emit<Func<ICommandHandler, ICommand, IValidator, CancellationToken, ValueTask<ICommandResponse>>>
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
        /// Bucket cache
        /// </summary>
        private sealed class CommandMethod
        {
            public Func<ICommandHandler, ICommand, CancellationToken, Task<ICommandResponse>> Handler;
            public Func<ICommandHandler, ICommand, IValidator, CancellationToken, ValueTask<ICommandResponse>> Validator;
            public Func<ICommandHandler, ICommand, CancellationToken, ValueTask<ICommandResponse>> Compliance;
        }
        #endregion

        #region Events
        private static Dictionary<Type, Func<IEventHandler, IEvent, CancellationToken, Task<IEventResponse>>> _eventCache;

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
}
