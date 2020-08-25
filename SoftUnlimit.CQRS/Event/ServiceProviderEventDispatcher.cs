using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using SoftUnlimit.CQRS.Message;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Event
{
    /// <summary>
    /// 
    /// </summary>
    public class ServiceProviderEventDispatcher : IEventDispatcherWithServiceProvider
    {
        private readonly bool _useCache, _useScope;
        private readonly IServiceProvider _provider;

        private static readonly object _sync = new object();
        private static readonly Dictionary<string, bool> _register = new Dictionary<string, bool>();
        private static readonly Dictionary<KeyPair, MethodInfo> _cache = new Dictionary<KeyPair, MethodInfo>();


        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="useCache"></param>
        /// <param name="useScope"></param>
        public ServiceProviderEventDispatcher(IServiceProvider provider, bool useCache = true, bool useScope = true)
        {
            this._provider = provider;
            this._useCache = useCache;
            this._useScope = useScope;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public Task<CombinedEventResponse> DispatchEventAsync(IEvent @event)
        {
            if (!this._useScope)
                return this.DispatchEventAsync(this._provider, @event);

            using IServiceScope scope = this._provider.CreateScope();
            return this.DispatchEventAsync(scope.ServiceProvider, @event);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="event"></param>
        /// <returns></returns>
        public Task<CombinedEventResponse> DispatchEventAsync(IServiceProvider provider, IEvent @event)
        {
            //
            // get handler and execute event.
            Type eventType = @event.GetType();
            List<Task<EventResponse>> eventTasks = new List<Task<EventResponse>>();

            IEnumerable<IEventHandler> handlers = GetHandlers(provider, @event.GetType());
            if (handlers?.Any() != true)
                return Task.FromResult(CombinedEventResponse.Empty);

            if (this._useCache)
            {
                foreach (var handler in handlers)
                {
                    KeyPair key = new KeyPair(eventType, handler.GetType());
                    var method = GetFromCache(key, handler, true);

                    try
                    {
                        var task = (Task<EventResponse>)method.Invoke(handler, new object[] { @event });
                        var taskResponse = task.ContinueWith(c => {
                            if (c.Exception != null)
                                return @event.ErrorResponse(c.Exception.InnerException ?? c.Exception);
                            return c.Result;
                        });
                        eventTasks.Add(taskResponse);
                    } catch (Exception ex)
                    {
                        var response = @event.ErrorResponse(ex.InnerException ?? ex);
                        var taskResponse = Task.FromResult(response);
                        eventTasks.Add(taskResponse);
                    }
                }
                return Task.WhenAll(eventTasks).ContinueWith(c => new CombinedEventResponse(c.Result));
            }

            foreach (var handler in handlers)
                eventTasks.Add(((dynamic)handler).HandleAsync((dynamic)@event));
            return Task.WhenAll(eventTasks).ContinueWith(c => new CombinedEventResponse(c.Result));
        }

        #region Static Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="handler"></param>
        /// <param name="isAsync"></param>
        /// <returns></returns>
        private static MethodInfo GetFromCache(KeyPair key, object handler, bool isAsync)
        {
            if (!_cache.TryGetValue(key, out MethodInfo method))
                lock (_sync)
                {
                    if (!_cache.ContainsKey(key))
                    {
                        method = handler
                            .GetType()
                            .GetMethod(nameof(IEventHandler<IEvent>.HandleAsync), new Type[] { key.Event });
                        if (method == null)
                            throw new KeyNotFoundException($"Not found event handler, is Async: {isAsync} for {handler}");

                        _cache.Add(key, method);
                    }
                }
            return method;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="scopeProvider"></param>
        /// <param name="eventType"></param>
        /// <returns></returns>
        private static IEnumerable<IEventHandler> GetHandlers(IServiceProvider scopeProvider, Type eventType)
        {
            Type handlerType = typeof(IEventHandler<>).MakeGenericType(eventType);
            return scopeProvider.GetServices(handlerType).Cast<IEventHandler>();
        }

        /// <summary>
        /// If exist handler asociate with this event return true, false in other case.
        /// </summary>
        /// <param name="eventName"></param>
        /// <returns></returns>
        public static bool HasHadler(string eventName) => _register.ContainsKey(eventName);
        /// <summary>
        /// Register EventHandler in DPI.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="eventHandlerInterface">Interface used to register tha binding between command handler and command.</param>
        /// <param name="eventAssembly"></param>
        public static void RegisterEventHandler(IServiceCollection services, Type eventHandlerInterface, params Assembly[] eventAssembly)
        {
            List<Type> existEventHandler = new List<Type>();
            foreach (var assembly in eventAssembly)
            {
                var query = assembly
                    .GetTypes()
                    .Where(p => p.IsClass && p.IsAbstract == false && p.GetInterfaces().Contains(typeof(IEventHandler)));
                existEventHandler.AddRange(query);
            }

            foreach (var commandHandlerImplementation in existEventHandler)
            {
                var commandHandlerImplementedInterfaces = commandHandlerImplementation.GetInterfaces()
                    .Where(p =>
                        p.IsGenericType &&
                        p.GetGenericArguments().Length == 1 &&
                        p.GetGenericTypeDefinition() == eventHandlerInterface);

                foreach (var handlerInterface in commandHandlerImplementedInterfaces)
                {
                    var eventType = handlerInterface.GetGenericArguments().Single();
                    var wellKnowCommandHandlerInterface = typeof(IEventHandler<>).MakeGenericType(eventType);

                    if (!_register.ContainsKey(eventType.FullName))
                        _register.Add(eventType.FullName, true);
                    services.AddScoped(wellKnowCommandHandlerInterface, commandHandlerImplementation);
                }
            }
        }

        #endregion

        #region Nested Clasess

        private sealed class KeyPair
        {
            public KeyPair(Type @event, Type handler)
            {
                Event = @event;
                Handler = handler;
            }

            public Type Event { get; }
            public Type Handler { get; }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;
                if (obj is KeyPair other)
                    return this.Event == other.Event && this.Handler == other.Handler;
                return false;
            }
            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode() => this.Event.GetHashCode() ^ (this.Handler.GetHashCode() >> 32);
        }

        #endregion
    }
}
