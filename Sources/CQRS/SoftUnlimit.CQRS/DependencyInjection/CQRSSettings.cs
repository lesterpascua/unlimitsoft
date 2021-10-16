using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.EventSourcing;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SoftUnlimit.CQRS.DependencyInjection
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class CQRSSettings
    {
        /// <summary>
        /// Assemblies used to scan for CommandHandler, QueryHandler and EventHandler
        /// </summary>
        public IEnumerable<Assembly> Assemblies { get; set; }
        /// <summary>
        /// typeof(IQueryAsyncHandler&lt;,&gt;)
        /// </summary>
        public Type IQueryHandler { get; set; }

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
        public Func<IServiceProvider, IEventDispatcher> EventDispatcher { get; set; }
        /// <summary>
        /// Trigger some action before dispatch command
        /// </summary>
        public Action<IServiceProvider, ICommand> PreeDispatchAction { get; set; }
    }
}
