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
using System.Collections.Generic;
using System.Linq;
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

            var events = EventBusInterceptor();
            var pending = await PopulateDatabase("Document1", pendingRepository, externalStorage);

            await unitOfWork.SaveChangesAsync();

            // Act
            var command = new ScanRequestCommand(Guid.NewGuid(), new IdentityInfo(), pending.Id);
            var response = await dispatcher.DispatchAsync(scope.ServiceProvider, command);

            SpinWait.SpinUntil(() => events.Event1 is not null && events.Event2 is not null);


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

            events.Other.Should().BeNull();

            // Assert RequestCompleteEvent
            events.Event1.SourceId.Should().Be(pending.Id);
            events.Event1.CorrelationId.Should().Be(pending.CorrelationId);
            ((RequestCompleteBody)events.Event1.Body).CustomerId.Should().Be(pending.CustomerId);
            ((RequestCompleteBody)events.Event1.Body).RequestId.Should().Be(pending.RequestId);
            ((RequestCompleteBody)events.Event1.Body).BlobUri.Should().Be(pending.BlobUri);
            ((RequestCompleteBody)events.Event1.Body).Metadata.Should().Be(pending.Metadata);
            ((RequestCompleteBody)events.Event1.Body).DownloadStatus.Should().Be(StorageStatus.Success);
            ((RequestCompleteBody)events.Event1.Body).ScanStatus.Should().Be(ScanStatus.Clean);
            ((RequestCompleteBody)events.Event1.Body).Success.Should().BeTrue();

            // Assert DocumentCreateEvent
            events.Event2.SourceId.Should().Be(pending.Id);
            events.Event2.CorrelationId.Should().Be(pending.CorrelationId);
            ((DocumentCreateBody)events.Event2.Body).CustomerId.Should().Be(pending.CustomerId);
            ((DocumentCreateBody)events.Event2.Body).DocumentId.Should().Be(pending.RequestId);
            ((DocumentCreateBody)events.Event2.Body).BlobUri.Should().Be(pending.BlobUri);
            ((DocumentCreateBody)events.Event2.Body).Metadata.Should().Be(pending.Metadata);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task DispatchCommand_ProcessMultiplesCleanFile_MoveToCompleteTableMoveToCleanStoragePublishEventDocumentCreateANDRequestComplete()
        {
            using var scope = _appFactory.Services.CreateScope();

            // Arrange
            var startOn = DateTime.UtcNow;
            var dispatcher = scope.ServiceProvider.GetService<ICommandDispatcher>();
            var unitOfWork = scope.ServiceProvider.GetService<ICloudUnitOfWork>();
            var pendingRepository = scope.ServiceProvider.GetService<ICloudRepository<Pending>>();
            var completeRepository = scope.ServiceProvider.GetService<ICloudRepository<Complete>>();
            var externalStorage = scope.ServiceProvider.GetService<IExternalStorage>();

            var events = EventBusInterceptor();

            var requests = new Pending[1000];
            for (int i = 0; i < requests.Length; i++)
                requests[i] = await PopulateDatabase(Guid.NewGuid().ToString(), pendingRepository, externalStorage);

            await unitOfWork.SaveChangesAsync();

            // Act
            var tasks = requests
                .Select(s => dispatcher.DispatchAsync(new ScanRequestCommand(Guid.NewGuid(), new IdentityInfo(), s.Id)))
                .ToArray();
            Task.WaitAll(tasks);


            // Assert
            for (int i = 0; i < requests.Length; i++)
            {
                var request = requests[0];
                var pending = await scope.ServiceProvider.GetService<ICloudQueryRepository<Pending>>().FindAll().FirstOrDefaultAsync(p => p.Id == request.Id);
                var complete = await scope.ServiceProvider.GetService<ICloudQueryRepository<Complete>>().FindAll().FirstOrDefaultAsync(p => p.Id == request.Id);

                pending.Should().BeNull();

                complete.CustomerId.Should().Be(request.CustomerId);
                complete.RequestId.Should().Be(request.RequestId);
                complete.CorrelationId.Should().Be(request.CorrelationId);
                complete.BlobUri.Should().Be(request.BlobUri);
                complete.ScanStatus.Should().Be(ScanStatus.Clean);
                complete.DownloadStatus.Should().Be(StorageStatus.Success);
                complete.Created.Should().BeOnOrBefore(DateTime.UtcNow);

                complete.Scanned.Should().BeOnOrAfter(startOn);
                complete.Scanned.Should().BeOnOrBefore(DateTime.UtcNow);

                complete.Retry.Should().Be(request.Retry);
            }

            //cleanFile.Should().NotBeNull();
            //cleanStatus.Should().Be(StorageStatus.Success);

            //pendingFile.Should().BeNull();
            //pendingStatus.Should().Be(StorageStatus.NotFound);

            //events.Other.Should().BeNull();

            // Assert RequestCompleteEvent
            //events.Event1.SourceId.Should().Be(pending.Id);
            //events.Event1.CorrelationId.Should().Be(pending.CorrelationId);
            //((RequestCompleteBody)events.Event1.Body).CustomerId.Should().Be(pending.CustomerId);
            //((RequestCompleteBody)events.Event1.Body).RequestId.Should().Be(pending.RequestId);
            //((RequestCompleteBody)events.Event1.Body).BlobUri.Should().Be(pending.BlobUri);
            //((RequestCompleteBody)events.Event1.Body).Metadata.Should().Be(pending.Metadata);
            //((RequestCompleteBody)events.Event1.Body).DownloadStatus.Should().Be(StorageStatus.Success);
            //((RequestCompleteBody)events.Event1.Body).ScanStatus.Should().Be(ScanStatus.Clean);
            //((RequestCompleteBody)events.Event1.Body).Success.Should().BeTrue();

            // Assert DocumentCreateEvent
            //events.Event2.SourceId.Should().Be(pending.Id);
            //events.Event2.CorrelationId.Should().Be(pending.CorrelationId);
            //((DocumentCreateBody)events.Event2.Body).CustomerId.Should().Be(pending.CustomerId);
            //((DocumentCreateBody)events.Event2.Body).DocumentId.Should().Be(pending.RequestId);
            //((DocumentCreateBody)events.Event2.Body).BlobUri.Should().Be(pending.BlobUri);
            //((DocumentCreateBody)events.Event2.Body).Metadata.Should().Be(pending.Metadata);
        }

        private EventArray<RequestCompleteEvent, DocumentCreateEvent> EventBusInterceptor()
        {
            var eventBus = _appFactory.Services.GetService<EventBusFake>();
            var events = new EventArray<RequestCompleteEvent, DocumentCreateEvent>();

            eventBus.Action = (eventId, eventName, eventPayload, correlation, index) =>
            {
                if (eventName == typeof(RequestCompleteEvent).FullName)
                {
                    events.Event1 = JsonUtility.Deserialize<RequestCompleteEvent>(eventPayload.ToString());
                }
                else if (eventName == typeof(DocumentCreateEvent).FullName)
                {
                    events.Event2 = JsonUtility.Deserialize<DocumentCreateEvent>(eventPayload.ToString());
                }
                else
                    events.Other = eventName;
            };
            return events;
        }
        private static async Task<Pending> PopulateDatabase(string blobUri, ICloudRepository<Pending> pendingRepository, IExternalStorage externalStorage)
        {
            var pending = new Pending
            {
                BlobUri = blobUri,
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
            await externalStorage.UploadAsync(pending.BlobUri, Setup.GetNonVirusFile(), StorageType.Pending);
            return pending;
        }
    }

    public class EventArray<T1, T2>
    {
        public object Other { get; set; }
        public T1 Event1 { get; set; }
        public T2 Event2 { get; set; }
    }
}
