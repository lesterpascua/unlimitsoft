using Microsoft.Extensions.Options;
using SoftUnlimit.Cloud.Event;
using SoftUnlimit.Cloud.Partner.Domain.Handler.Command;
using SoftUnlimit.Cloud.Security;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.Message;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.Cloud.Partner.Domain.Handler.Events
{
    public class CreateGenericCloudEvent : CloudEvent<object>
    {
        public CreateGenericCloudEvent(Guid id, Guid sourceId, long version, ushort serviceId, string workerId, string correlationId, object command, object prevState, object currState, bool isDomainEvent, object body) : 
            base(id, sourceId, version, serviceId, workerId, correlationId, command, prevState, currState, isDomainEvent, body)
        {
        }
    }

    public sealed class CreateGenericCloudEventHandler : ICloudEventHandler<CreateGenericCloudEvent>
    {
        private readonly IdentityInfo _identity;
        private readonly IServiceProvider _provider;
        private readonly ICommandDispatcher _dispatcher;

        public CreateGenericCloudEventHandler(IServiceProvider provider, ICommandDispatcher dispatcher, IOptions<AuthorizeOptions> autorize)
        {
            _provider = provider;
            _dispatcher = dispatcher;
            _identity = autorize.Value.User;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="event"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<IEventResponse> HandleAsync(CreateGenericCloudEvent @event, CancellationToken ct = default)
        {
            var eventIdentity = @event.GetIdentity(_identity);

            var command = new SaveEventInPendingTableCommand(@event.Id, eventIdentity, @event);
            var response = await _dispatcher.DispatchAsync(_provider, command, ct);

            return @event.OkResponse(response.GetBody());
        }
    }
}
