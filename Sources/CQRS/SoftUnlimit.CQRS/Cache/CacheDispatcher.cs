using Sigil;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Message;
using SoftUnlimit.CQRS.Query;
using SoftUnlimit.Web.Event;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Cache
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class CacheDispatcher
    {
        private static object _querySync;
        private static Dictionary<Type, Func<object, IQuery, CancellationToken, Task>> _queryCache;

        private static object _commandSync;
        private static Dictionary<Type, Func<object, ICommand, CancellationToken, Task<ICommandResponse>>> _commandCache;

        private static object _eventSync;
        private static Dictionary<Type, Func<object, IEvent, CancellationToken, Task<IEventResponse>>> _eventCache;

        private const string HandleAsync = nameof(HandleAsync);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="useCache"></param>
        protected CacheDispatcher(bool useCache = true)
        {
            UseCache = useCache;
        }


        /// <summary>
        /// 
        /// </summary>
        protected bool UseCache { get; }


        private static object QuerySync
        {
            get
            {
                if (_querySync == null)
                    Interlocked.CompareExchange(ref _querySync, new object(), null);
                return _querySync;
            }
        }
        private static object CommandSync
        {
            get
            {
                if (_commandSync == null)
                    Interlocked.CompareExchange(ref _commandSync, new object(), null);
                return _commandSync;
            }
        }
        private static object EventSync
        {
            get
            {
                if (_eventSync == null)
                    Interlocked.CompareExchange(ref _eventSync, new object(), null);
                return _eventSync;
            }
        }
        private static Dictionary<Type, Func<object, IQuery, CancellationToken, Task>> QueryCache
        {
            get
            {
                if (_queryCache == null)
                    Interlocked.CompareExchange(ref _queryCache, new Dictionary<Type, Func<object, IQuery, CancellationToken, Task>>(), null);
                return _queryCache;
            }
        }
        private static Dictionary<Type, Func<object, ICommand, CancellationToken, Task<ICommandResponse>>> CommandCache
        {
            get
            {
                if (_commandCache == null)
                    Interlocked.CompareExchange(ref _commandCache, new Dictionary<Type, Func<object, ICommand, CancellationToken, Task<ICommandResponse>>>(), null);
                return _commandCache;
            }
        }
        private static Dictionary<Type, Func<object, IEvent, CancellationToken, Task<IEventResponse>>> EventCache
        {
            get
            {
                if (_eventCache == null)
                    Interlocked.CompareExchange(ref _eventCache, new Dictionary<Type, Func<object, IEvent, CancellationToken, Task<IEventResponse>>>(), null);
                return _eventCache;
            }
        }


        /// <summary>
        /// Check the method HandlerAsync asociate to the type specified. 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        protected static Func<object, IQuery, CancellationToken, Task> GetQueryHandlerFromCache<TResult>(Type type, object handler)
        {
            var cache = QueryCache;
            if (!cache.TryGetValue(type, out Func<object, IQuery, CancellationToken, Task> @delegate))
            {
                lock (QuerySync)
                    if (!cache.TryGetValue(type, out @delegate))
                    {
                        var method = handler
                            .GetType()
                            .GetMethod(HandleAsync, new Type[] { type, typeof(CancellationToken) });
                        if (method == null)
                            throw new KeyNotFoundException($"Not found handler for {handler}");

                        var handlerType = handler.GetType();
                        @delegate = Emit<Func<object, IQuery, CancellationToken, Task<TResult>>>
                            .NewDynamicMethod($"{HandleAsync}_{type.FullName}")
                            .LoadArgument(0).CastClass(handlerType)
                            .LoadArgument(1).CastClass(type)
                            .LoadArgument(2)
                            .Call(method)
                            .Return()
                            .CreateDelegate();

                        cache.Add(type, @delegate);
                    }
            }
            return @delegate;
        }
        /// <summary>
        /// Check the method HandlerAsync asociate to the type specified. 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        protected static Func<object, ICommand, CancellationToken, Task<ICommandResponse>> GetCommandHandlerFromCache(Type type, object handler)
        {
            var cache = CommandCache;
            if (!cache.TryGetValue(type, out Func<object, ICommand, CancellationToken, Task<ICommandResponse>> @delegate))
            {
                lock (CommandSync)
                    if (!cache.TryGetValue(type, out @delegate))
                    {
                        var method = handler
                            .GetType()
                            .GetMethod(HandleAsync, new Type[] { type, typeof(CancellationToken) });
                        if (method == null)
                            throw new KeyNotFoundException($"Not found handler for {handler}");

                        var handlerType = handler.GetType();
                        @delegate = Emit<Func<object, ICommand, CancellationToken, Task<ICommandResponse>>>
                            .NewDynamicMethod($"{HandleAsync}_{type.FullName}")
                            .LoadArgument(0).CastClass(handlerType)
                            .LoadArgument(1).CastClass(type)
                            .LoadArgument(2)
                            .Call(method)
                            .Return()
                            .CreateDelegate();

                        cache.Add(type, @delegate);
                    }
            }
            return @delegate;
        }
        /// <summary>
        /// Check the method HandlerAsync asociate to the type specified. 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        protected static Func<object, IEvent, CancellationToken, Task<IEventResponse>> GetEventHandlerFromCache(Type type, object handler)
        {
            var cache = EventCache;
            if (!cache.TryGetValue(type, out Func<object, IEvent, CancellationToken, Task<IEventResponse>> @delegate))
            {
                lock (EventSync)
                    if (!cache.TryGetValue(type, out @delegate))
                    {
                        var method = handler
                            .GetType()
                            .GetMethod(HandleAsync, new Type[] { type, typeof(CancellationToken) });
                        if (method == null)
                            throw new KeyNotFoundException($"Not found handler for {handler}");

                        var handlerType = handler.GetType();
                        @delegate = Emit<Func<object, IEvent, CancellationToken, Task<IEventResponse>>>
                            .NewDynamicMethod($"{HandleAsync}_{type.FullName}")
                            .LoadArgument(0).CastClass(handlerType)
                            .LoadArgument(1).CastClass(type)
                            .LoadArgument(2)
                            .Call(method)
                            .Return()
                            .CreateDelegate();

                        cache.Add(type, @delegate);
                    }
            }
            return @delegate;
        }
    }
}
