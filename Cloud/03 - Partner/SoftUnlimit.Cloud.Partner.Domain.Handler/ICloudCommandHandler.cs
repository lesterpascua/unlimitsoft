using SoftUnlimit.Cloud.Command;
using SoftUnlimit.CQRS.Command;

namespace SoftUnlimit.Cloud.Partner.Domain.Handler
{

    /// <summary>
    /// Used to identified command handler for this service.
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    public interface ICloudCommandHandler<TCommand> : ICommandHandler<TCommand>
         where TCommand : CloudCommand
    {
    }
}
