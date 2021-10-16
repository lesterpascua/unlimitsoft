using FluentAssertions;
using Hangfire;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using SoftUnlimit.Cloud.Antivirus;
using SoftUnlimit.Cloud.Security;
using SoftUnlimit.Cloud.Storage;
using SoftUnlimit.Cloud.VirusScan.Client;
using SoftUnlimit.Cloud.VirusScan.Data;
using SoftUnlimit.Cloud.VirusScan.Data.Model;
using SoftUnlimit.Cloud.VirusScan.Domain;
using SoftUnlimit.Cloud.VirusScan.Domain.Handler.Command;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.Json;
using SoftUnlimit.Web.AspNet.Testing;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SoftUnlimit.Cloud.VirusScan.WebApi.Tests
{
    public class ScanRequestCommandTest : IDisposable
    {
        private readonly IBackgroundJobClient _bgJobClient;
        private readonly WebApplicationFactory<Startup> _appFactory;

        public ScanRequestCommandTest()
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
        /// 
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task DispatchCommand_ProcessCleanFile_MoveToCompleteTableMoveToCleanStoragePublishEventDocumentCreateANDRequestComplete()
        {
            using var scope = _appFactory.Services.CreateScope();

            // Arrange
            var startOn = DateTime.UtcNow;
            var dispatcher = scope.ServiceProvider.GetService<ICommandDispatcher>();
            var unitOfWork = scope.ServiceProvider.GetService<ICloudUnitOfWork>();
            var pendingRepository = scope.ServiceProvider.GetService<ICloudRepository<Pending>>();
            var completeRepository = scope.ServiceProvider.GetService<ICloudRepository<Complete>>();
            var externalStorage = scope.ServiceProvider.GetService<IExternalStorage>();

            var pending = new Pending
            {
                BlobUri = "Document1",
                CorrelationId = Guid.NewGuid().ToString(),
                Created = DateTime.UtcNow,
                CustomerId = Guid.NewGuid(),
                RequestId = Guid.NewGuid(),
                Id = Guid.NewGuid(),
                Metadata = null,
                Retry = 1,
                Scheduler = DateTime.UtcNow,
                Status = StatusValues.Approved
            };
            await pendingRepository.AddAsync(pending);
            await unitOfWork.SaveChangesAsync();
            await externalStorage.UploadAsync(pending.BlobUri, Setup.GetNonVirusFile(), StorageType.Pending);

            object other = null;
            DocumentCreateEvent documentCreateEvent = null;
            RequestCompleteEvent requestCompleteEvent = null;
            var eventBus = _appFactory.Services.GetService<EventBusFake>();
            eventBus.Action = (eventId, eventName, eventPayload, correlation, index) =>
            {
                if (eventName == typeof(RequestCompleteEvent).FullName)
                {
                    requestCompleteEvent = JsonUtility.Deserialize<RequestCompleteEvent>(eventPayload.ToString());
                }
                else if (eventName == typeof(DocumentCreateEvent).FullName)
                {
                    documentCreateEvent = JsonUtility.Deserialize<DocumentCreateEvent>(eventPayload.ToString());
                }
                else
                    other = eventName;
            };

            // Act
            var command = new ScanRequestCommand(Guid.NewGuid(), new IdentityInfo(), pending.Id);
            var response = await dispatcher.DispatchAsync(scope.ServiceProvider, command);

            SpinWait.SpinUntil(() => documentCreateEvent is not null && requestCompleteEvent is not null);


            // Assert
            var request = await scope.ServiceProvider.GetService<ICloudQueryRepository<Pending>>().FindAll().FirstOrDefaultAsync();
            var complete = await scope.ServiceProvider.GetService<ICloudQueryRepository<Complete>>().FindAll().FirstOrDefaultAsync();
            var (cleanFile, cleanStatus) = await externalStorage.DownloadAsync(pending.BlobUri, StorageType.Clean);
            var (pendingFile, pendingStatus) = await externalStorage.DownloadAsync(pending.BlobUri, StorageType.Pending);

            response.IsSuccess.Should().BeTrue();
            request.Should().BeNull();

            complete.CustomerId.Should().Be(pending.CustomerId);
            complete.RequestId.Should().Be(pending.RequestId);
            complete.CorrelationId.Should().Be(pending.CorrelationId);
            complete.BlobUri.Should().Be(pending.BlobUri);
            complete.ScanStatus.Should().Be(ScanStatus.Clean);
            complete.DownloadStatus.Should().Be(StorageStatus.Success);
            complete.Created.Should().BeOnOrBefore(DateTime.UtcNow);

            complete.Scanned.Should().BeOnOrAfter(startOn);
            complete.Scanned.Should().BeOnOrBefore(DateTime.UtcNow);

            complete.Retry.Should().Be(pending.Retry);

            cleanFile.Should().NotBeNull();
            cleanStatus.Should().Be(StorageStatus.Success);

            pendingFile.Should().BeNull();
            pendingStatus.Should().Be(StorageStatus.NotFound);

            other.Should().BeNull();

            // Assert RequestCompleteEvent
            requestCompleteEvent.SourceId.Should().Be(pending.Id);
            requestCompleteEvent.CorrelationId.Should().Be(pending.CorrelationId);
            ((RequestCompleteBody)requestCompleteEvent.Body).CustomerId.Should().Be(pending.CustomerId);
            ((RequestCompleteBody)requestCompleteEvent.Body).RequestId.Should().Be(pending.RequestId);
            ((RequestCompleteBody)requestCompleteEvent.Body).BlobUri.Should().Be(pending.BlobUri);
            ((RequestCompleteBody)requestCompleteEvent.Body).Metadata.Should().Be(pending.Metadata);
            ((RequestCompleteBody)requestCompleteEvent.Body).DownloadStatus.Should().Be(StorageStatus.Success);
            ((RequestCompleteBody)requestCompleteEvent.Body).ScanStatus.Should().Be(ScanStatus.Clean);
            ((RequestCompleteBody)requestCompleteEvent.Body).Success.Should().BeTrue();

            // Assert DocumentCreateEvent
            documentCreateEvent.SourceId.Should().Be(pending.Id);
            documentCreateEvent.CorrelationId.Should().Be(pending.CorrelationId);
            ((DocumentCreateBody)documentCreateEvent.Body).CustomerId.Should().Be(pending.CustomerId);
            ((DocumentCreateBody)documentCreateEvent.Body).DocumentId.Should().Be(pending.RequestId);
            ((DocumentCreateBody)documentCreateEvent.Body).BlobUri.Should().Be(pending.BlobUri);
            ((DocumentCreateBody)documentCreateEvent.Body).Metadata.Should().Be(pending.Metadata);
        }
    }
}
