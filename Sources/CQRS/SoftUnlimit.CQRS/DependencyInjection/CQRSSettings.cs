using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Message;
using SoftUnlimit.CQRS.Query;
using SoftUnlimit.Event;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.DependencyInjection;


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
    public Func<IQuery, Func<IQuery, CancellationToken, Task<IQueryResponse>>, CancellationToken, Task<IQueryResponse>> PreeDispatchQuery { get; set; }
    /// <summary>
    /// Trigger some action before dispatch command
    /// </summary>
    public Func<IServiceProvider, IEvent, Func<IServiceProvider, IEvent, CancellationToken, Task<IEventResponse>>, CancellationToken, Task<IEventResponse>> PreeDispatchEvent { get; set; }
    /// <summary>
    /// Trigger some action before dispatch command
    /// </summary>
    public Func<IServiceProvider, ICommand, Func<IServiceProvider, ICommand, CancellationToken, Task<ICommandResponse>>, CancellationToken, Task<ICommandResponse>> PreeDispatchCommand { get; set; }
}
