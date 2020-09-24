﻿using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.EventSourcing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.DependencyInjection
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class CQRSSettings
    {
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<Assembly> Assemblies { get; set; }
        /// <summary>
        /// typeof(IQueryAsyncHandler)
        /// </summary>
        public Type IQueryAsyncHandler { get; set; }
        /// <summary>
        /// typeof(IQueryAsyncHandler&lt;,&gt;)
        /// </summary>
        public Type IQueryAsyncHandlerGeneric { get; set; }

        /// <summary>
        /// typeof(ICommandCompliance&lt;&gt;)
        /// </summary>
        public Type ICommandCompliance { get; set; }
        /// <summary>
        /// typeof(ICommandHandler&lt;&gt;)
        /// </summary>
        public Type ICommandHandler { get; set; }
        /// <summary>
        /// typeof(IEventHandler&lt;&gt;)
        /// </summary>
        public Type IEventHandler { get; set; }

        /// <summary>
        /// Materialized type derived of <see cref="IMediatorDispatchEventSourced"/> interface. 
        /// </summary>
        public Type MediatorDispatchEventSourced { get; set; }
        /// <summary>
        /// Event dispatcher used to handler events.
        /// </summary>
        public Func<IServiceProvider, IEventDispatcherWithServiceProvider> EventDispatcher { get; set; }
    }
}