﻿using Akka.Actor;
using Akka.Routing;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.AkkaBus.Properties;
using UnlimitSoft.CQRS.Command;
using UnlimitSoft.Mediator;
using UnlimitSoft.Message;

namespace UnlimitSoft.AkkaBus;


/// <summary>
/// Implement bus using akka actor system 
/// </summary>
public class AkkaCommandBus : ICommandBus
{
    private readonly bool _isOwner;
    private readonly Func<ICommand, Task> _preeSendCommand;
    private readonly static Dictionary<string, string[]> GENERIC_ERROR = new() { [string.Empty] = new string[] { Resources.Text_GenericError } };

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dispatcher"></param>
    /// <param name="process">Perform proceess command operation if null the behaivor is default.</param>
    /// <param name="factory">If null create new actor system, in other case use the stablish actor system.</param>
    /// <param name="instances"></param>
    /// <param name="logger"></param>
    /// <param name="options"></param>
    /// <param name="preeSendCommand">Before send the command invoke this method.</param>
    public AkkaCommandBus(
        ICommandDispatcher dispatcher, 
        Func<ICommandDispatcher, CommandEnvelopment, IActorRef, ILogger, Task> process = null,
        IActorRefFactory factory = null, 
        int instances = 2, 
        ILogger<AkkaCommandBus> logger = null,
        Action<IActorRefFactory> options = null,
        Func<ICommand, Task> preeSendCommand = null)
    {
        _isOwner = factory == null;

        Factory = factory ?? ActorSystem.Create("CommandBus");
        _preeSendCommand = preeSendCommand;
        Default = Factory.ActorOf(
            Props.Create(() => new CoordinatorActor(dispatcher, process ?? ProcessCommand, logger, instances)));

        if (options != null)
            options.Invoke(Factory);
    }

    /// <summary>
    /// Actor in change of process all command.
    /// </summary>
    public IActorRef Default { get; }
    /// <summary>
    /// Reference to the actor system.
    /// </summary>
    public IActorRefFactory Factory { get; }

    /// <summary>
    /// Dispatch command.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public virtual async Task<object> SendAsync(ICommand command, CancellationToken ct)
    {
        var envelopment = new CommandEnvelopment(command, false);
        if (_preeSendCommand != null)
            await _preeSendCommand(command);

        Default.Tell(envelopment);
        return null;
    }

    /// <summary>
    /// Dispose actor system and release resources.
    /// </summary>
    public void Dispose()
    {
        if (_isOwner && Factory is IDisposable disposable)
            disposable.Dispose();
    }

    #region Public Static Method

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dispatcher"></param>
    /// <param name="envelopment"></param>
    /// <param name="sender"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public static async Task ProcessCommand(ICommandDispatcher dispatcher, CommandEnvelopment envelopment, IActorRef sender, ILogger logger)
    {
        Exception err = null;
        IResult response;
        try
        {
            response = await dispatcher.DispatchDynamicAsync(envelopment.Command);
        } catch (Exception exc)
        {
            err = exc;
            response = Result.FromError<object>(envelopment.Command.ErrorResponse(GENERIC_ERROR));
            logger.LogError(exc, "CoordinatorActor.DispatchAsync: {Command}", envelopment.Command);
        }

        if (envelopment.IsAsk)
            sender.Tell(response);
    }

    #endregion

    #region Nested Classes

    /// <summary>
    /// Process special command type using workers child.
    /// </summary>
    private class CoordinatorActor : ReceiveActor
    {
        private readonly IActorRef _router;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="dispatcher">Command dispatcher.</param>
        /// <param name="instances">Number of worker child used for this actor.</param>
        /// <param name="commandAction"></param>
        /// <param name="logger"></param>
        public CoordinatorActor(
            ICommandDispatcher dispatcher,
            Func<ICommandDispatcher, CommandEnvelopment, IActorRef, ILogger, Task> commandAction,
            ILogger logger,
            int instances)
        {
            _router = Context.ActorOf(
                Props.Create(() => new WorkerActor(dispatcher, commandAction, logger))
                    .WithRouter(new SmallestMailboxPool(instances)),
                "child");

            Receive<CommandEnvelopment>(command => _router.Forward(command));
        }
    }
    /// <summary>
    /// Worker thread for parent coordinator actor.
    /// </summary>
    private class WorkerActor : ReceiveActor
    {
        private readonly ILogger _logger;
        private readonly ICommandDispatcher _dispatcher;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="dispatcher">Command dispatcher.</param>
        /// <param name="action"></param>
        /// <param name="logger"></param>
        public WorkerActor(
            ICommandDispatcher dispatcher,
            Func<ICommandDispatcher, CommandEnvelopment, IActorRef, ILogger, Task> action,
            ILogger logger)
        {
            _logger = logger;
            _dispatcher = dispatcher;

            ReceiveAsync<CommandEnvelopment>(envelopment => action.Invoke(_dispatcher, envelopment, Sender, _logger));
        }
    }

    #endregion
}
