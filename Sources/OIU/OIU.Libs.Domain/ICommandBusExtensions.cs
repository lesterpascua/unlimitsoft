using OIU.Libs.Data;
using UnlimitSoft.CQRS.Command;

namespace OIU.Libs.Domain;


/// <summary>
/// Extensions for the ICommandBus interface.
/// </summary>
public static class ICommandBusExtensions
{
    extension (ICommandBus @this)
    {
        /// <summary>
        /// Sent a command and set the job id to the entity. This is useful for tracking the job status later.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="entity"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<string?> SendAsync(ICommand command, IEntityWithJob entity, CancellationToken ct)
        {
            var jobId = await @this.SendAsync(command, ct);
            entity.BusJobId = jobId?.ToString();

            return entity.BusJobId;
        }
    }
}
