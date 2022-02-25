using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.Query;
using SoftUnlimit.Event;
using System;
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
        public Assembly[] Assemblies { get; set; }

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
        /// Trigger some action before dispatch command
        /// </summary>
        public Action<IServiceProvider, IEvent> PreeDispatchEvent { get; set; }
        /// <summary>
        /// Trigger some action before dispatch command
        /// </summary>
        public Action<IServiceProvider, ICommand> PreeDispatchCommand { get; set; }
        /// <summary>
        /// Trigger some action before dispatch command
        /// </summary>
        public Action<IServiceProvider, IQuery> PreeDispatchQuery { get; set; }
    }
}
