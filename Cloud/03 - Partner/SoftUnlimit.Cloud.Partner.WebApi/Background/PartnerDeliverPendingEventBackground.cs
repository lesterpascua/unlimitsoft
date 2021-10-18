using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftUnlimit.Cloud.Partner.Domain.Handler;
using SoftUnlimit.Cloud.Security;
using SoftUnlimit.Cloud.Security.Cryptography;
using SoftUnlimit.CQRS.Command;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.Cloud.Partner.WebApi.Background
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class PartnerDeliverPendingEventBackground : BackgroundService
    {
        private readonly IdentityInfo _identity;
        private readonly PartnerValues _partnerId;
        private readonly ICloudIdGenerator _gen;
        private readonly ICommandDispatcher _dispatcher;
        private readonly ILogger _logger;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="partnerId"></param>
        /// <param name="gen"></param>
        /// <param name="authorizeOptions"></param>
        /// <param name="dispatcher"></param>
        /// <param name="logger"></param>
        protected PartnerDeliverPendingEventBackground(
            PartnerValues partnerId,
            ICloudIdGenerator gen,
            IOptions<AuthorizeOptions> authorizeOptions,
            ICommandDispatcher dispatcher,
            ILogger logger)
        {
            _identity = authorizeOptions.Value.User;
            _partnerId = partnerId;
            _gen = gen;
            _dispatcher = dispatcher;
            _logger = logger;
        }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            await Task.Delay(TimeSpan.FromMinutes(1), ct);
            _logger.LogInformation("Start publich events to external partners");

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var command = new DeliverPendingEventToPartnerCommand(_gen.GenerateId(), _identity) { PartnerId = _partnerId };
                    var response = await _dispatcher.DispatchAsync(command, ct);
                    if (!response.IsSuccess)
                        _logger.LogInformation("Error deliver pending event to partner {@Result}", response.GetBody());

                    _logger.LogInformation("Check pending event ends with: {Result}", response.GetBody());
                }
                catch (Exception ex)
                {
                    _logger.LogInformation(ex, "Error deliver pending event to partner");
                }

                await Task.Delay(TimeSpan.FromSeconds(10), ct);
            }
        }
    }
}
