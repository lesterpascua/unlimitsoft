using Akka.Actor;
using Akka.Routing;
using Microsoft.Extensions.Logging;
using SoftUnlimit.AkkaBus.Properties;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Message;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SoftUnlimit.AkkaBus
{
    /// <summary>
    /// Implement bus using akka actor system 
    /// </summary>
    public class CommandBus : ICommandBus
    {
        private readonly bool _isOwner;
        private readonly static Dictionary<string, string[]> GENERIC_ERROR = new Dictionary<string, string[]> {
            [string.Empty] = new string[] { Resources.Text_GenericError }
        };


        /// <summary>
        /// 
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <param name="completionService"></param>
        /// <param name="process">Perform proceess command operation if null the behaivor is default.</param>
        /// <param name="factory">If null create new actor system, in other case use the stablish actor system.</param>
        /// <param name="instances"></param>
        /// <param name="logger"></param>
        /// <param name="options"></param>
        public CommandBus(
            ICommandDispatcher dispatcher, 
            ICommandCompletionService completionService, 
            Func<ICommandDispatcher, CommandEnvelopment, IActorRef, ICommandCompletionService, ILogger, Task> process = null,
            IActorRefFactory factory = null, 
            int instances = 2, 
            ILogger<CommandBus> logger = null,
            Action<IActorRefFactory> options = null)
        {
            this._isOwner = factory == null;

            this.Factory = factory ?? ActorSystem.Create("CommandBus");
            this.Default = this.Factory.ActorOf(
                Props.Create(() => new CoordinatorActor(
                    dispatcher, completionService, process ?? ProcessCommand, logger, instances)));

            if (options != null)
                options.Invoke(this.Factory);
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
        /// <returns></returns>
        public virtual Task SendAsync(ICommand command)
        {
            var envelopment = new CommandEnvelopment(command, false);
            this.Default.Tell(envelopment);

            return Task.CompletedTask;
        }
        /// <summary>
        /// Dispatch command and wait for response.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public virtual Task<CommandResponse> SendAndWaitAsync(ICommand command)
        {
            var envelopment = new CommandEnvelopment(command, true);
            return this.Default.Ask<CommandResponse>(envelopment);
        }

        /// <summary>
        /// Dispose actor system and release resources.
        /// </summary>
        public void Dispose()
        {
            if (this._isOwner && this.Factory is IDisposable disposable)
                disposable.Dispose();
        }

        #region Public Static Method

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <param name="envelopment"></param>
        /// <param name="sender"></param>
        /// <param name="completionService"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static async Task ProcessCommand(ICommandDispatcher dispatcher, CommandEnvelopment envelopment, IActorRef sender, ICommandCompletionService completionService, ILogger logger)
        {
            CommandResponse response;
            try
            {
                response = await dispatcher.DispatchAsync(envelopment.Command);
            } catch (Exception exc)
            {
                response = envelopment.Command.ErrorResponse(GENERIC_ERROR);
                logger.LogError(exc, "CoordinatorActor.DispatchAsync: {0}", envelopment.Command);
            }

            if (envelopment.IsAsk)
                sender.Tell(response);
            if (completionService != null && !envelopment.Command.GetProps<CommandProps>().Silent)
                await completionService.SendAsync(response, false);
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
            /// <param name="completionService">Send response throw this service.</param>
            /// <param name="instances">Number of worker child used for this actor.</param>
            /// <param name="commandAction"></param>
            /// <param name="logger"></param>
            public CoordinatorActor(
                ICommandDispatcher dispatcher,
                ICommandCompletionService completionService,
                Func<ICommandDispatcher, CommandEnvelopment, IActorRef, ICommandCompletionService, ILogger, Task> commandAction,
                ILogger logger,
                int instances)
            {
                this._router = Context.ActorOf(
                    Props.Create(() => new WorkerActor(dispatcher, completionService, commandAction, logger))
                        .WithRouter(new SmallestMailboxPool(instances)),
                    "child");

                this.Receive<CommandEnvelopment>(command => this._router.Forward(command));
            }
        }
        /// <summary>
        /// Worker thread for parent coordinator actor.
        /// </summary>
        private class WorkerActor : ReceiveActor
        {
            private readonly ILogger _logger;
            private readonly ICommandDispatcher _dispatcher;
            private readonly ICommandCompletionService _completionService;


            /// <summary>
            /// 
            /// </summary>
            /// <param name="dispatcher">Command dispatcher.</param>
            /// <param name="completionService">Send response throw this service.</param>
            /// <param name="action"></param>
            /// <param name="logger"></param>
            public WorkerActor(
                ICommandDispatcher dispatcher,
                ICommandCompletionService completionService,
                Func<ICommandDispatcher, CommandEnvelopment, IActorRef, ICommandCompletionService, ILogger, Task> action,
                ILogger logger)
            {
                this._logger = logger;
                this._dispatcher = dispatcher;
                this._completionService = completionService;

                this.ReceiveAsync<CommandEnvelopment>(envelopment =>
                    action.Invoke(this._dispatcher, envelopment, this.Sender, this._completionService, this._logger));
            }
        }

        #endregion
    }
}
