using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftUnlimit.CQRS.Cache;
using SoftUnlimit.CQRS.Message;
using SoftUnlimit.Web.Event;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Event
{
    /// <summary>
    /// 
    /// </summary>
    public class ServiceProviderEventDispatcher : CacheDispatcher, IEventDispatcher
    {
        private readonly bool _useCache, _useScope;
        private readonly Action<IServiceProvider, IEvent> _preeDispatch;
        private readonly ILogger<ServiceProviderEventDispatcher> _logger;
        private readonly IServiceProvider _provider;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="useCache"></param>
        /// <param name="useScope"></param>
        /// <param name="preeDispatch"></param>
        /// <param name="logger"></param>
        public ServiceProviderEventDispatcher(IServiceProvider provider, bool useCache = true, bool useScope = true,
            Action<IServiceProvider, IEvent> preeDispatch = null, ILogger<ServiceProviderEventDispatcher> logger = null
        )
            : base(useCache)
        {
            _provider = provider;
            _useCache = useCache;
            _useScope = useScope;
            _preeDispatch = preeDispatch;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<IEventResponse> DispatchAsync(IEvent @event, CancellationToken ct = default)
        {
            if (!_useScope)
                return await DispatchAsync(_provider, @event, ct);

            using IServiceScope scope = _provider.CreateScope();
            return await DispatchAsync(scope.ServiceProvider, @event, ct);
        }
        /// <inheritdoc />
        public async Task<IEventResponse> DispatchAsync(IServiceProvider provider, IEvent @event, CancellationToken ct = default)
        {
            _logger?.LogDebug("Process event: {@Event}", @event);
            _preeDispatch?.Invoke(provider, @event);

            //
            // get handler and execute event.
            Type eventType = @event.GetType();

            var handler = GetEventHandler(provider, eventType);
            return await ExecuteHandlerForCommandAsync(handler, @event, eventType, UseCache, ct);
        }
        #region Static Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scopeProvider"></param>
        /// <param name="eventType"></param>
        /// <returns></returns>
        private static IEventHandler GetEventHandler(IServiceProvider scopeProvider, Type eventType)
        {
            Type serviceType = typeof(IEventHandler<>).MakeGenericType(eventType);
            var eventHandler = (IEventHandler)scopeProvider.GetService(serviceType);
            if (eventHandler == null)
                throw new KeyNotFoundException("There is no handler associated with this event");

            return eventHandler;
        }
        private static async Task<IEventResponse> ExecuteHandlerForCommandAsync(IEventHandler handler, IEvent e, Type commandType, bool useCache, CancellationToken ct)
        {
            Task<IEventResponse> result;
            if (useCache)
            {
                var method = GetEventHandlerFromCache(commandType, handler);
                result = method(handler, e, ct);
            }
            else
                result = (Task<IEventResponse>)((dynamic)handler).HandleAsync((dynamic)e, ct);

            return await result;
        }
        #endregion
    }
}
