using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.EventSourcing.Json;
using SoftUnlimit.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SoftUnlimit.Tests.SoftUnlimit.CQRS.Event
{
    public class QueueEventPublishWorkerTests
    {
        private readonly IEventBus _eventBus;
        private readonly IServiceScopeFactory _factory;
        private readonly JsonVersionedEventPayload[] _data;
        private readonly List<JsonVersionedEventPayload> _publish;

        public QueueEventPublishWorkerTests()
        {
            _publish = new List<JsonVersionedEventPayload>();
            _data = new JsonVersionedEventPayload[] {
                new JsonVersionedEventPayload{ Id = Guid.NewGuid(), Created = new DateTime(1, 12, 31), Scheduled = new DateTime(9999, 12, 31) },
                new JsonVersionedEventPayload{ Id = Guid.NewGuid(), Created = new DateTime(2021, 10, 10), Scheduled = new DateTime(2021, 10, 12) },
                new JsonVersionedEventPayload{ Id = Guid.NewGuid(), Created = new DateTime(2021, 10, 11), Scheduled = null },
            };

            _eventBus = Substitute.For<IEventBus>();
            _eventBus
                .PublishPayloadAsync(Arg.Any<JsonVersionedEventPayload>(), MessageType.Event, Arg.Any<CancellationToken>())
                .Returns(args =>
                {
                    _publish.Add(args.ArgAt<JsonVersionedEventPayload>(0));
                    return Task.CompletedTask;
                });

            var repository = Substitute.For<IRepository<JsonVersionedEventPayload>>();
            repository.FindAll().Returns(_data.AsQueryable());

            var provider = Substitute.For<IServiceProvider>();
            provider
                .GetService(Arg.Any<Type>())
                .Returns(info =>
                {
                    if (info.ArgAt<Type>(0) == typeof(IUnitOfWork))
                        return Substitute.For<IUnitOfWork>();
                    if (info.ArgAt<Type>(0) == typeof(IRepository<JsonVersionedEventPayload>))
                        return repository;

                    throw new NotSupportedException();
                });

            var scope = Substitute.For<IServiceScope>();
            scope.ServiceProvider.Returns(provider);

            _factory = Substitute.For<IServiceScopeFactory>();
            _factory.CreateScope().Returns(scope);
        }

        [Fact]
        public async Task StartLoadingPendingEvent_Load3EventOnly2SchedulerInTime_ShouldPublish2EventInEventBus()
        {
            // Arrange
            using var publishWorker = new QueueEventPublishWorker<IUnitOfWork, IRepository<JsonVersionedEventPayload>, JsonVersionedEventPayload, string>(_factory, _eventBus, MessageType.Event, TimeSpan.Zero, TimeSpan.Zero, 10, true, null);


            // Act
            await publishWorker.StartAsync(true);


            // Assert
            SpinWait.SpinUntil(() => _publish.Count == 2, TimeSpan.FromSeconds(5));
            _publish.Count.Should().Be(2);
            _publish.Should().Contain(_data[1]);
            _publish.Should().Contain(_data[2]);
        }
    }
}
