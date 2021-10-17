using SoftUnlimit.Cloud.Command;
using SoftUnlimit.Cloud.Partner.Domain.Handler.Events;
using SoftUnlimit.Cloud.Security;
using SoftUnlimit.CQRS.Message;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.Cloud.Partner.Domain.Handler.Command
{
    public class SaveEventInPendingTableCommand : CloudCommand
    {
        public SaveEventInPendingTableCommand(Guid id, IdentityInfo user, CreateGenericCloudEvent @event) : 
            base(id, user)
        {
            Event = @event;
        }

        public CreateGenericCloudEvent Event { get; set; }
    }
    public sealed class SaveEventInPendingTableCommandHandler : 
        ICloudCommandHandler<SaveEventInPendingTableCommand>
    {
        public Task<ICommandResponse> HandleAsync(SaveEventInPendingTableCommand command, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
