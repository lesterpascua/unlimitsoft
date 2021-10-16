using Microsoft.Extensions.Options;
using SoftUnlimit.Cloud.Event;
using SoftUnlimit.Cloud.Security;
using SoftUnlimit.Cloud.VirusScan.Domain.Handler.Command;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.Message;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.Cloud.VirusScan.Domain.Handler.Event
{
    public sealed class CreateRequestEventHandler : ICloudEventHandler<RequestCreateEvent>
    {
        private readonly IdentityInfo _identity;
        private readonly IServiceProvider _provider;
        private readonly ICommandDispatcher _dispatcher;

        public CreateRequestEventHandler(IServiceProvider provider, ICommandDispatcher dispatcher, IOptions<AuthorizeOptions> autorize)
        {
            _provider = provider;
            _dispatcher = dispatcher;
            _identity = autorize.Value.User;
        }

        /// <summary>
        /// Get event for some request. Dispatch a <see cref="CreateRequestCommand"/> to save in the database and start the background scan.
        /// </summary>
        /// <param name="event"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<IEventResponse> HandleAsync(RequestCreateEvent @event, CancellationToken ct = default)
        {
            var body = @event.GetBody<RequestCreateBody>();
            var eventIdentity = @event.GetIdentity(_identity);

            var command = new CreateRequestCommand(@event.Id, eventIdentity, body);
            var response = await _dispatcher.DispatchAsync(_provider, command, ct);

            return @event.OkResponse(response.GetBody());
        }
    }
}
