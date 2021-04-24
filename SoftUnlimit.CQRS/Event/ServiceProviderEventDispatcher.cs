using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftUnlimit.CQRS.Command;
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
    public class ServiceProviderEventDispatcher : IEventDispatcher
    {
        private readonly bool _useCache, _useScope;
        private readonly ILogger<ServiceProviderEventDispatcher> _logger;
        private readonly IServiceProvider _provider;

        private static readonly object _sync = new object();
        private static readonly Dictionary<string, Type> _register = new Dictionary<string, Type>();
        private static readonly Dictionary<KeyPair, MethodInfo> _cache = new Dictionary<KeyPair, MethodInfo>();


        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="useCache"></param>
        /// <param name="useScope"></param>
        /// <param name="logger"></param>
        public ServiceProviderEventDispatcher(IServiceProvider provider, bool useCache = true, bool useScope = true, ILogger<ServiceProviderEventDispatcher> logger = null)
        {
            _provider = provider;
            _useCache = useCache;
            _useScope = useScope;
            _logger = logger;
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
            _logger?.LogDebug("Process event: {@Event}", @event);

            //
            // get handler and execute event.
            Type eventType = @event.GetType();
            List<Task<EventResponse>> eventTasks = new List<Task<EventResponse>>();

            IEnumerable<IEventHandler> handlers = GetHandlers(provider, eventType);
            if (handlers?.Any() != true)
            {
                _logger?.LogDebug("Not fount handler for event: {Event}", eventType);
                return Task.FromResult(CombinedEventResponse.Empty);
            }


            _logger?.LogDebug("Found {Count} handlers for event: {Type}", handlers.Count(), eventType);
            if (_useCache)
            {
                _logger?.LogDebug("Event cache event is enable");
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
                return Task.WhenAll(eventTasks).ContinueWith(c =>
                {
                    _logger?.LogDebug("Event handler responses: {@Responses}", c.Result);
                    return new CombinedEventResponse(c.Result);
                });
            }
            else
                _logger?.LogDebug("Event cache event is disabled");

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
        /// Get collection of register events.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<KeyValuePair<string, Type>> GetRegisterEvents()
        {
            return _register.Select(s => new KeyValuePair<string, Type>(s.Key, _register[s.Key]));
        }
        /// <summary>
        /// Get a dictionaty with a mapping of event name and (eventType, commandType). 
        /// </summary>
        /// <returns></returns>
        public static IReadOnlyDictionary<string, Type> GetTypeResolver()
        {
            Dictionary<string, Type> cache = new Dictionary<string, Type>();
            foreach (var regEvent in GetRegisterEvents())
            {
                cache.Add(regEvent.Key, regEvent.Value);

                //var contructors = regEvent.Value.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
                //var ctor = contructors.FirstOrDefault(p => p.GetParameters().Length == 10);
                //if (ctor != null)
                //{
                //    var commandParam = ctor.GetParameters()[5];
                //    var commandType = commandParam.ParameterType;
                //    if (commandType.GetInterface(nameof(ICommand)) != null)
                //        cache.Add(regEvent.Key, regEvent.Value);
                //}
            }
            return cache;
        }
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

            foreach (var eventHandlerImplementation in existEventHandler)
            {
                var eventHandlerImplementedInterfaces = eventHandlerImplementation.GetInterfaces()
                    .Where(p =>
                        p.IsGenericType &&
                        p.GetGenericArguments().Length == 1 &&
                        p.GetGenericTypeDefinition() == eventHandlerInterface);

                foreach (var handlerInterface in eventHandlerImplementedInterfaces)
                {
                    var eventType = handlerInterface.GetGenericArguments().Single();
                    var wellKnowCommandHandlerInterface = typeof(IEventHandler<>).MakeGenericType(eventType);

                    if (!_register.ContainsKey(eventType.FullName))
                        _register.Add(eventType.FullName, eventType);
                    services.AddScoped(wellKnowCommandHandlerInterface, eventHandlerImplementation);
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
