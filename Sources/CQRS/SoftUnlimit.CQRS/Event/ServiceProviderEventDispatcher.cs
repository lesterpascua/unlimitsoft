﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftUnlimit.CQRS.Cache;
using SoftUnlimit.CQRS.Message;
using SoftUnlimit.Event;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Event
{
    /// <summary>
    /// 
    /// </summary>
    public class ServiceProviderEventDispatcher : IEventDispatcher
    {
        private readonly IServiceProvider _provider;

        private readonly bool _useScope;
        private readonly Action<IServiceProvider, IEvent> _preeDispatch;

        private readonly ILogger<ServiceProviderEventDispatcher> _logger;

        private readonly Dictionary<Type, Type> _cache;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="useScope"></param>
        /// <param name="preeDispatch"></param>
        /// <param name="logger"></param>
        public ServiceProviderEventDispatcher(IServiceProvider provider, bool useScope = true,
            Action<IServiceProvider, IEvent> preeDispatch = null, ILogger<ServiceProviderEventDispatcher> logger = null
        )
        {
            _provider = provider;
            _useScope = useScope;
            _preeDispatch = preeDispatch;
            _logger = logger;
            _cache = new Dictionary<Type, Type>();
        }

        /// <inheritdoc />
        public async Task<IEventResponse> DispatchAsync(IEvent @event, CancellationToken ct = default)
        {
            if (!_useScope)
                return await DispatchAsync(_provider, @event, ct);

            using var scope = _provider.CreateScope();
            return await DispatchAsync(scope.ServiceProvider, @event, ct);
        }
        /// <inheritdoc />
        public async Task<IEventResponse> DispatchAsync(IServiceProvider provider, IEvent @event, CancellationToken ct = default)
        {
            _preeDispatch?.Invoke(provider, @event);
            _logger?.LogDebug("Process event: {@Event}", @event);

            //
            // get handler and execute event.
            var eventType = @event.GetType();
            _logger?.LogDebug("Execute event type: {Type}", eventType);

            var handler = GetEventHandler(provider, eventType);
            if (handler is not null)
                return await HandlerAsync(handler, @event, eventType, ct);

            _logger.LogWarning("There is no handler associated with this event");
            return @event.OkResponse();
        }
        #region Static Methods

        /// <summary>
        /// Get event handler and metadata asociate to a event.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="eventType"></param>
        /// <returns></returns>
        private IEventHandler GetEventHandler(IServiceProvider provider, Type eventType)
        {
            if (_cache.TryGetValue(eventType, out var handleType))
                return (IEventHandler)provider.GetRequiredService(handleType);

            lock (_cache)
                if (!_cache.TryGetValue(eventType, out handleType))
                    _cache.Add(eventType, handleType = typeof(IEventHandler<>).MakeGenericType(eventType));

            return (IEventHandler)provider.GetRequiredService(handleType);
        }
        private Task<IEventResponse> HandlerAsync(IEventHandler handler, IEvent @event, Type commandType, CancellationToken ct)
        {
            var method = CacheDispatcher.GetEventHandler(commandType, handler);
            return method(handler, @event, ct);
        }
        #endregion
    }
}
