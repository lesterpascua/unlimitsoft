using FluentAssertions;
using SoftUnlimit.Web;
using Hangfire;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SoftUnlimit.Cloud.Bus;
using SoftUnlimit.Cloud.Partner.Data;
using SoftUnlimit.Cloud.Partner.Data.Model;
using SoftUnlimit.Cloud.Partner.Domain.Handler.Events;
using SoftUnlimit.CQRS.Event.Json;
using SoftUnlimit.Json;
using SoftUnlimit.Security;
using SoftUnlimit.Web.AspNet.Testing;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SoftUnlimit.Cloud.Partner.WebApi.Tests
{
    public class CreateGenericCloudEventHandlerIntegrationTest : IDisposable
    {
        private readonly WebApplicationFactory<Startup> _appFactory;


        public CreateGenericCloudEventHandlerIntegrationTest()
        {
            _appFactory = TestFactory.Factory<Startup>(false);
        }

        public void Dispose()
        {
            _appFactory.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Process event, save event in database, scheduler event job.
        /// </summary>
        /// <returns></returns>
        [Fact(Skip = "Integration")]
        [Trait("Category", "Integration")]
        public async Task PushEvent_EventDataIsCorrect_SaveEventInPendingTableAndExchedulerTheScanning()
        {
            using var scope = _appFactory.Services.CreateScope();

            // Arrange
            var startOn = DateTime.UtcNow;
            var metadata = scope.ServiceProvider.GetService<IServiceMetadata>();
            var unitOfWork = scope.ServiceProvider.GetService<ICloudUnitOfWork>();
            var pendingQueryRepository = scope.ServiceProvider.GetService<ICloudQueryRepository<SaleforcePending>>();
            var completeQueryRepository = scope.ServiceProvider.GetService<ICloudQueryRepository<SaleforceComplete>>();

            var configuration = scope.ServiceProvider.GetService<IConfiguration>();
            var eventNameResolver = scope.ServiceProvider.GetService<IEventNameResolver>();
            var busEndpoint = configuration.GetConnectionString("Endpoint");

            var bus = new EventBus.Azure.AzureEventBus<QueueIdentifier>(
                busEndpoint,
                new QueueAlias[] { new QueueAlias { Active = true, Alias = QueueIdentifier.Partner, Queue = QueueIdentifier.Partner.ToPrettyString() } }, 
                eventNameResolver
            );
            await bus.StartAsync(TimeSpan.FromSeconds(1));


            // Act
            var body = new { Test = "asdsa" };
            var @event = ListenerFake.CreateEvent<CreateGenericCloudEvent>(metadata, body);
            @event.Name = "TestEvent";
            @event.PartnerId = PartnerValues.JnReward;
            await bus.PublishAsync(@event);

            await Task.Delay(TimeSpan.FromSeconds(5));

            // Assert
            var pending = await pendingQueryRepository.FindAll().FirstAsync();
            var complete = await completeQueryRepository.FindAll().FirstOrDefaultAsync();

            complete.Should().BeNull();

            pending.Body.Should().Be(JsonUtility.Serialize(body));
            pending.CorrelationId.Should().Be(@event.CorrelationId);

            pending.Created.Should().BeAfter(startOn);
            pending.Created.Should().BeBefore(DateTime.UtcNow);

            pending.EventId.Should().Be(@event.Id);
            pending.IdentityId.Should().Be(@event.IdentityId);
            pending.Name.Should().Be(@event.Name);
            pending.PartnerId.Should().Be(@event.PartnerId);
            pending.Retry.Should().Be(0);

            pending.Scheduler.Should().BeAfter(startOn);
            pending.Scheduler.Should().BeBefore(DateTime.UtcNow);

            pending.ServiceId.Should().Be(@event.ServiceId);
            pending.SourceId.Should().Be(@event.SourceId.ToString());
            pending.Version.Should().Be(@event.Version);
            pending.WorkerId.Should().Be(@event.WorkerId);
        }
    }
}
