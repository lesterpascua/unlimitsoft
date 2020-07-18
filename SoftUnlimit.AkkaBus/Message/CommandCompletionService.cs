﻿using Akka.Actor;
using Microsoft.Extensions.Logging;
using SoftUnlimit.CQRS.Message;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.AkkaBus.Message
{
    /// <summary>
    /// 
    /// </summary>
    public class CommandCompletionService : ICommandCompletionService
    {
        private readonly IActorRefFactory _factory;
        private readonly IActorRef _commandCompletionActor;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="actorPath"></param>
        public CommandCompletionService(IActorRefFactory factory, string actorPath)
        {
            //"akka.tcp://ServerAkka@localhost:8081/user/RemoteActor"
            this._factory = factory;
            this._commandCompletionActor = this._factory.ActorOf(Props.Create(() => new DispatchActor(actorPath)));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="response"></param>
        /// <param name="urgent"></param>
        /// <returns></returns>
        public Task SendAsync(CommandResponse response, bool urgent)
        {
            this._commandCompletionActor.Tell(response);
            return Task.CompletedTask;
        }

        #region Private Methods 

        /// <summary>
        /// Dispatch response throws the actor.
        /// </summary>
        private class DispatchActor : ReceiveActor
        {
            private readonly ActorSelection _remoteActorRef;


            /// <summary>
            /// 
            /// </summary>
            /// <param name="actorPath"></param>
            public DispatchActor(string actorPath)
            {
                this._remoteActorRef = Context.ActorSelection(actorPath);
                this.Receive<CommandResponse>(response => this._remoteActorRef.Tell(response));
            }
        }

        #endregion
    }
}