using Microsoft.Extensions.DependencyInjection;
using MockQueryable.Moq;
using Moq;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.Event.Json;
using SoftUnlimit.CQRS.EventSourcing;
using SoftUnlimit.CQRS.EventSourcing.Json;
using SoftUnlimit.Data;
using SoftUnlimit.Map;
using SoftUnlimit.Web.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SoftUnlimit.CQRS.Tests
{
    /// <summary>
    /// 
    /// </summary>
    public interface ITestVersionedEventRepository : IRepository<JsonVersionedEventPayload>
    {
    }
    public sealed class TestQueueEventPublishWorker : QueueEventPublishWorker<IUnitOfWork, ITestVersionedEventRepository, JsonVersionedEventPayload, string>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="eventBus"></param>
        /// <param name="type"></param>
        public TestQueueEventPublishWorker(
            IServiceProvider provider, 
            IEventBus eventBus, 
            MessageType type
        )
            : base(provider, eventBus, type, null)
        {
        }
    }

    public sealed class TestEvent : VersionedEvent<Guid>
    {
        public TestEvent(Guid id, Guid sourceId, long version, uint serviceId, string workerId, string correlationId, ICommand command, object prevState, object currState, bool isDomainEvent, IEventBodyInfo body = null)
            : base(id, sourceId, version, serviceId, workerId, correlationId, command, prevState, currState, isDomainEvent, body)
        {
        }
    }
    public sealed class TestEntity
    {
        public int Value { get; set; }
        public string Text { get; set; }
    }

    public class QueueEventPublishWorkerTest
    {
        public IUnitOfWork FakeUnitOfWork(Action action)
        {
            var fakeUnitOfWork = new Mock<IUnitOfWork>();
            fakeUnitOfWork
                .Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(() => {
                    action?.Invoke();
                    return Task.FromResult(1);
                });

            return fakeUnitOfWork.Object;
        }
        public IEventBus FakeEventBus(Action<JsonVersionedEventPayload, MessageType> action)
        {
            var fakeEventBus = new Mock<IEventBus>();
            fakeEventBus
                .Setup(s => s.PublishEventPayloadAsync(It.IsAny<JsonVersionedEventPayload>(), It.Is<MessageType>(p => p == MessageType.Json)))
                .Returns<JsonVersionedEventPayload, MessageType>((payload, type) => {
                    action?.Invoke(payload, type);
                    return Task.CompletedTask;
                });

            return fakeEventBus.Object;
        }
        public ITestVersionedEventRepository FakeVersionedEventRepository(IQueryable<JsonVersionedEventPayload> data)
        {
            var fakeVersionesEventRepository = new Mock<ITestVersionedEventRepository>();
            fakeVersionesEventRepository
                .Setup(s => s.FindAll())
                .Returns(() => data);

            return fakeVersionesEventRepository.Object;
        }


        [Fact]
        public async Task Publish_Event_And_Destroy_Queue()
        {
            // Arrange
            bool eventPublised = false;
            bool saveTransaction = false;
            var event1 = new TestEvent(
                Guid.Parse("8900D168-469D-4A13-B866-1A9B726CA040"), 
                Guid.Parse("8900D168-469D-4A13-B866-1A9B726CA049"), 
                1,
                20, 
                "12345",
                "adsd",
                null, 
                new TestEntity{ Text ="prev", Value = 10 }, 
                new TestEntity{ Text ="curr", Value = 11 },
                true,
                null);

            var fakeEventsRepository = new List<JsonVersionedEventPayload> { new JsonVersionedEventPayload(event1) }.AsQueryable().BuildMock().Object;
            var fakeUnitOfWork = FakeUnitOfWork(() => saveTransaction = true);
            var fakeVersionesEventRepository = FakeVersionedEventRepository(fakeEventsRepository);
            var fakeEventBus = FakeEventBus((payload, type) => {
                Assert.Equal(MessageType.Json, type);
                Assert.Equal(fakeEventsRepository.First(), payload);
                eventPublised = true;
            });

            var services = new ServiceCollection();
            services.AddScoped(p => fakeUnitOfWork);
            services.AddScoped(p => fakeVersionesEventRepository);
            using var provider = services.BuildServiceProvider();

            var queue = new TestQueueEventPublishWorker(provider, fakeEventBus, MessageType.Json);

            // Act
            queue.Publish(new IEvent[] { event1 });

            SpinWait.SpinUntil(() => eventPublised);
            await queue.DisposeAsync();

            bool throwException = false;
            try
            {
                queue.Publish(null);
            } catch (ObjectDisposedException)
            {
                throwException = true;
            }

            // Assert
            Assert.True(saveTransaction);
            Assert.True(eventPublised);
            Assert.True(throwException);
        }
    }
}
