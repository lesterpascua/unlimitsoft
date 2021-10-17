using FluentAssertions;
using Hangfire;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SoftUnlimit.Cloud.Partner.Data;
using SoftUnlimit.Cloud.Partner.Data.Model;
using SoftUnlimit.Cloud.Partner.Domain.Handler.Events;
using SoftUnlimit.Json;
using SoftUnlimit.Web.AspNet.Testing;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SoftUnlimit.Cloud.Partner.WebApi.Tests
{
    public class CreateGenericCloudEventHandlerTest : IDisposable
    {
        private readonly WebApplicationFactory<Startup> _appFactory;


        public CreateGenericCloudEventHandlerTest()
        {
            _appFactory = Setup.Factory();
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
        [Fact]
        public async Task PushEvent_EventDataIsCorrect_SaveEventInPendingTableAndExchedulerTheScanning()
        {
            using var scope = _appFactory.Services.CreateScope();

            // Arrange
            var startOn = DateTime.UtcNow;
            var unitOfWork = scope.ServiceProvider.GetService<ICloudUnitOfWork>();
            var pendingQueryRepository = scope.ServiceProvider.GetService<ICloudQueryRepository<SaleforcePending>>();
            var completeQueryRepository = scope.ServiceProvider.GetService<ICloudQueryRepository<SaleforceComplete>>();


            // Act
            var body = new { Test = "asdsa" };
            var listener = _appFactory.Services.GetService<ListenerFake>();

            var @event = listener.CreateEvent<CreateGenericCloudEvent>(body);
            @event.Name = "TestEvent";
            @event.PartnerId = PartnerValues.JnReward;
            var (eventResponse, exc) = await listener.SimulateReceiveAsync(@event);


            // Assert
            var pending = await pendingQueryRepository.FindAll().FirstAsync();
            var complete = await completeQueryRepository.FindAll().FirstOrDefaultAsync();

            exc.Should().BeNull();
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
