using FluentAssertions;
using Hangfire;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using SoftUnlimit.Cloud.VirusScan.Client;
using SoftUnlimit.Cloud.VirusScan.Data;
using SoftUnlimit.Cloud.VirusScan.Data.Model;
using SoftUnlimit.Cloud.VirusScan.Domain;
using SoftUnlimit.Security;
using SoftUnlimit.Web.AspNet.Testing;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SoftUnlimit.Cloud.VirusScan.WebApi.Tests
{
    public class CreateRequestEventTest : IDisposable
    {
        private readonly IBackgroundJobClient _bgJobClient;
        private readonly WebApplicationFactory<Startup> _appFactory;


        public CreateRequestEventTest()
        {
            _bgJobClient = Substitute.For<IBackgroundJobClient>();
            _appFactory = Setup.Factory(services =>
            {
                services.RemoveAll<IBackgroundJobClient>();
                services.AddSingleton(_bgJobClient);
            });
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
            var metadata = scope.ServiceProvider.GetService<IServiceMetadata>();
            var unitOfWork = scope.ServiceProvider.GetService<ICloudUnitOfWork>();
            var pendingQueryRepository = scope.ServiceProvider.GetService<ICloudQueryRepository<Pending>>();
            var completeQueryRepository = scope.ServiceProvider.GetService<ICloudQueryRepository<Complete>>();


            // Act
            var body = new RequestCreateBody(new RequestInfo[] {
                new RequestInfo(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid().ToString())
            });
            var listener = _appFactory.Services.GetService<ListenerFake>();

            var @event = ListenerFake.CreateEvent<RequestCreateEvent>(metadata, body);
            var (eventResponse, exc) = await listener.SimulateReceiveAsync(@event);


            // Assert
            var request = await pendingQueryRepository.FindAll().FirstAsync();
            var complete = await completeQueryRepository.FindAll().FirstOrDefaultAsync();

            exc.Should().BeNull();
            complete.Should().BeNull();

            request.CustomerId.Should().Be(body.Requests[0].CustomerId);
            request.RequestId.Should().Be(body.Requests[0].RequestId);
            request.CorrelationId.Should().Be(@event.CorrelationId);
            request.BlobUri.Should().Be(body.Requests[0].BlobUri);
            request.Status.Should().Be(StatusValues.Approved);

            request.Created.Should().BeOnOrAfter(startOn);
            request.Created.Should().BeOnOrBefore(DateTime.UtcNow);

            request.Scheduler.Should().Be(request.Created);
            request.Retry.Should().Be(0);
            request.Metadata.Should().BeNull();
        }
    }
}
