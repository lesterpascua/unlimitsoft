using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using NSubstitute;
using SoftUnlimit.Cloud.Partner.Data;
using SoftUnlimit.Cloud.Partner.Data.Model;
using SoftUnlimit.Cloud.Partner.Domain.Handler;
using SoftUnlimit.Cloud.Partner.Domain.Handler.Services.Saleforce;
using SoftUnlimit.Cloud.Partner.Saleforce.Sender;
using SoftUnlimit.Cloud.Partner.Saleforce.Sender.Model;
using SoftUnlimit.Cloud.Security;
using SoftUnlimit.CQRS.Command;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SoftUnlimit.Cloud.Partner.WebApi.Tests
{
    public class DeliverPendingEventToPartnerCommandTest : IDisposable
    {
        private readonly AuthorizeOptions _autorize;
        private readonly IEventPublisherApiService _eventPublisherApiServiceMock;
        private readonly IEventPublisherApiServiceFactory _eventPublisherApiServiceFactory;

        private readonly WebApplicationFactory<Startup> _appFactory;


        public DeliverPendingEventToPartnerCommandTest()
        {
            _eventPublisherApiServiceMock = Substitute.For<IEventPublisherApiService>();
            _eventPublisherApiServiceFactory = Substitute.For<IEventPublisherApiServiceFactory>();

            _appFactory = Setup.Factory(services => {
                services.RemoveAll<IEventPublisherApiServiceFactory>();
                services.AddSingleton(_eventPublisherApiServiceFactory);
            });
            _autorize = _appFactory.Services.GetService<IOptions<AuthorizeOptions>>().Value;
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
        public async Task DispatchCommand_WithOneRequestPendingForSaleforce_PublishEventAndMoveFromPendingToCompleteTable()
        {
            using var scope = _appFactory.Services.CreateScope();

            // Arrange
            var startOn = DateTime.UtcNow;
            var unitOfWork = scope.ServiceProvider.GetService<ICloudUnitOfWork>();
            var pendingRepository = scope.ServiceProvider.GetService<ICloudRepository<SaleforcePending>>();
            var completeRepository = scope.ServiceProvider.GetService<ICloudRepository<SaleforceComplete>>();
            var dispatcher = scope.ServiceProvider.GetService<ICommandDispatcher>();
            _eventPublisherApiServiceFactory.CreateOrGetAsync(Arg.Any<PartnerValues>(), Arg.Any<CancellationToken>()).Returns(_eventPublisherApiServiceMock);
            _eventPublisherApiServiceMock.PublishAsync(Arg.Any<EventSignature>(), Arg.Any<CancellationToken>()).Returns((new PublishStatus(Guid.NewGuid().ToString(), true, Array.Empty<Error>()), System.Net.HttpStatusCode.OK));

            var pending = new SaleforcePending
            {
                Body = "{\"Test\":\"asdsa\",\"SfRecordId\":\"asddsa\"}",
                CorrelationId = "365d1522-9fd6-46ae-b677-70a93f687b2d",
                Created = DateTime.UtcNow,
                EventId = Guid.Parse("442725BA-3C37-4121-BB40-D31EAA55835D"),
                IdentityId = null,
                Name = "TestEvent",
                PartnerId = null,
                Retry = 0,
                Scheduler = DateTime.UtcNow,
                ServiceId = 4,
                WorkerId = "ABVd7mF2",
                SourceId = "c88ea73c-81a9-4772-bd4a-c76bbca60e36",
                Version = 0
            };
            
            await pendingRepository.AddAsync(pending);
            await unitOfWork.SaveChangesAsync();


            // Act
            var command = new DeliverPendingEventToPartnerCommand(Guid.NewGuid(), _autorize.User) { PartnerId = PartnerValues.Saleforce };
            var response = await dispatcher.DispatchAsync(command);


            // Assert
            var currPending = await scope.ServiceProvider.GetService<ICloudQueryRepository<SaleforcePending>>().FindAll().FirstOrDefaultAsync();
            var currComplete = await scope.ServiceProvider.GetService<ICloudQueryRepository<SaleforceComplete>>().FindAll().FirstOrDefaultAsync();

            currPending.Should().BeNull();

            currComplete.Body.Should().Be(pending.Body);
            currComplete.CorrelationId.Should().Be(pending.CorrelationId);

            currComplete.Created.Should().BeAfter(startOn);
            currComplete.Created.Should().BeBefore(DateTime.UtcNow);

            currComplete.EventId.Should().Be(pending.EventId);
            currComplete.IdentityId.Should().Be(pending.IdentityId);
            currComplete.Name.Should().Be(pending.Name);
            currComplete.PartnerId.Should().Be(pending.PartnerId);
            currComplete.Retry.Should().Be(0);

            currComplete.Completed.Should().BeAfter(startOn);
            currComplete.Completed.Should().BeBefore(DateTime.UtcNow);

            currComplete.ServiceId.Should().Be(pending.ServiceId);
            currComplete.SourceId.Should().Be(pending.SourceId.ToString());
            currComplete.Version.Should().Be(pending.Version);
            currComplete.WorkerId.Should().Be(pending.WorkerId);

            await _eventPublisherApiServiceMock.Received(1).PublishAsync(Arg.Any<EventSignature>(), Arg.Any<CancellationToken>());
        }
        /// <summary>
        /// Process event, save event in database, scheduler event job.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task DispatchCommand_WithMultiplesRequestPendingForSaleforce_PublishAllEventAndMoveFromPendingToCompleteTable()
        {
            using var scope = _appFactory.Services.CreateScope();

            // Arrange
            var startOn = DateTime.UtcNow;
            var unitOfWork = scope.ServiceProvider.GetService<ICloudUnitOfWork>();
            var pendingRepository = scope.ServiceProvider.GetService<ICloudRepository<SaleforcePending>>();
            var completeRepository = scope.ServiceProvider.GetService<ICloudRepository<SaleforceComplete>>();
            var dispatcher = scope.ServiceProvider.GetService<ICommandDispatcher>();
            _eventPublisherApiServiceFactory.CreateOrGetAsync(Arg.Any<PartnerValues>(), Arg.Any<CancellationToken>()).Returns(_eventPublisherApiServiceMock);
            _eventPublisherApiServiceMock.PublishAsync(Arg.Any<EventSignature>(), Arg.Any<CancellationToken>()).Returns((new PublishStatus(Guid.NewGuid().ToString(), true, Array.Empty<Error>()), System.Net.HttpStatusCode.OK));

            var pendings = new SaleforcePending[101];
            for (int i = 0; i < pendings.Length; i++)
                pendings[i] = new SaleforcePending
                {
                    Body = $"{{\"Test\":\"asdsa\",\"SfRecordId\":\"{Guid.NewGuid()}\"}}",
                    CorrelationId = Guid.NewGuid().ToString(),
                    Created = DateTime.UtcNow,
                    EventId = Guid.NewGuid(),
                    IdentityId = null,
                    Name = "TestEvent",
                    PartnerId = null,
                    Retry = 0,
                    Scheduler = DateTime.UtcNow,
                    ServiceId = 4,
                    WorkerId = "ABVd7mF2",
                    SourceId = Guid.NewGuid().ToString(),
                    Version = 0
                };

            await pendingRepository.AddRangeAsync(pendings);
            await unitOfWork.SaveChangesAsync();


            // Act
            var command = new DeliverPendingEventToPartnerCommand(Guid.NewGuid(), _autorize.User) { PartnerId = PartnerValues.Saleforce };
            var response = await dispatcher.DispatchAsync(command);


            // Assert
            await _eventPublisherApiServiceMock
                .Received(pendings.Length)
                .PublishAsync(Arg.Any<EventSignature>(), Arg.Any<CancellationToken>());
            var currPending = await scope.ServiceProvider.GetService<ICloudQueryRepository<SaleforcePending>>().FindAll().FirstOrDefaultAsync();

            var completeQueryRepository = scope.ServiceProvider.GetService<ICloudQueryRepository<SaleforceComplete>>();
            for (int i = 0; i < pendings.Length; i++)
            {
                var pending = pendings[i];
                var currComplete = await completeQueryRepository.FindAll().FirstOrDefaultAsync(p => p.EventId == pending.EventId);

                currPending.Should().BeNull();

                currComplete.Body.Should().Be(pending.Body);
                currComplete.CorrelationId.Should().Be(pending.CorrelationId);

                currComplete.Created.Should().BeAfter(startOn);
                currComplete.Created.Should().BeBefore(DateTime.UtcNow);

                currComplete.EventId.Should().Be(pending.EventId);
                currComplete.IdentityId.Should().Be(pending.IdentityId);
                currComplete.Name.Should().Be(pending.Name);
                currComplete.PartnerId.Should().Be(pending.PartnerId);
                currComplete.Retry.Should().Be(0);

                currComplete.Completed.Should().BeAfter(startOn);
                currComplete.Completed.Should().BeBefore(DateTime.UtcNow);

                currComplete.ServiceId.Should().Be(pending.ServiceId);
                currComplete.SourceId.Should().Be(pending.SourceId.ToString());
                currComplete.Version.Should().Be(pending.Version);
                currComplete.WorkerId.Should().Be(pending.WorkerId);
            }
        }
    }
}
