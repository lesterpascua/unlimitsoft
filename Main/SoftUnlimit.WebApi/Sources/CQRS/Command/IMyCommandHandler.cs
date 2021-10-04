using SoftUnlimit.CQRS.Command;

namespace SoftUnlimit.WebApi.Sources.CQRS.Command
{

    /// <summary>
    /// Used to identified command handler for this service.
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    public interface IMyCommandHandler<TCommand> : ICommandHandler<TCommand>
         where TCommand : MyCommand
    {
    }
}
