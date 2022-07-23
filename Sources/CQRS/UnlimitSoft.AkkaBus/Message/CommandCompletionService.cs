using Akka.Actor;
using UnlimitSoft.CQRS.Command;
using UnlimitSoft.CQRS.Message;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace UnlimitSoft.AkkaBus.Message;


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
        _factory = factory;
        _commandCompletionActor = _factory.ActorOf(Props.Create(() => new DispatchActor(actorPath)));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="command"></param>
    /// <param name="response"></param>
    /// <param name="ex"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public Task<ICommandResponse> CompleteAsync(ICommand command, ICommandResponse response, Exception ex = null, CancellationToken ct = default)
    {
        _commandCompletionActor.Tell(response);
        return Task.FromResult(response);
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
            _remoteActorRef = Context.ActorSelection(actorPath);
            Receive<ICommandResponse>(response => _remoteActorRef.Tell(response));
        }
    }

    #endregion
}
