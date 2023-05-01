using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.Data;
using UnlimitSoft.CQRS.Data.Dto;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.CQRS.Event.Json;
using UnlimitSoft.Data;
using UnlimitSoft.Web.Model;
using Xunit;

namespace UnlimitSoft.Tests.UnlimitSoft.CQRS.Event;


public class QueueEventPublishWorkerTests
{
    private readonly IEventBus _eventBus;
    private readonly IServiceScopeFactory _factory;
    private readonly EventPayload[] _data;
    private readonly List<EventPayload> _publish;

    public QueueEventPublishWorkerTests()
    {
        _publish = new List<EventPayload>();
        _data = new EventPayload[] {
            new EventPayload{ Id = Guid.NewGuid(), Created = new DateTime(1, 12, 31), IsPubliched = true, Scheduled = DateTime.MaxValue },
            new EventPayload{ Id = Guid.NewGuid(), Created = new DateTime(2021, 10, 10), Scheduled = new DateTime(2021, 10, 12) },
            new EventPayload{ Id = Guid.NewGuid(), Created = new DateTime(2021, 10, 11), Scheduled = null },
        };

        _eventBus = Substitute.For<IEventBus>();
        _eventBus
            .PublishPayloadAsync(Arg.Any<EventPayload>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(args =>
            {
                _publish.Add(args.ArgAt<EventPayload>(0));
                return Task.CompletedTask;
            });

        var repository = Substitute.For<IEventRepository<EventPayload>>();
        repository.GetNonPublishedEventsAsync(Arg.Any<Paging>(), Arg.Any<CancellationToken>())
            .Returns(x =>
            {
                var paging = x.ArgAt<Paging>(0);
                return _data
                    .AsQueryable()
                    .Where(p => p.IsPubliched == false)
                    .Skip(paging.Page * paging.PageSize)
                    .Take(paging.PageSize)
                    .Select(s => new NonPublishEventPayload(s.Id, s.SourceId, s.Version, s.Created, s.Scheduled))
                    .ToList();
            });
        repository.GetEventsAsync(Arg.Any<Guid[]>(), Arg.Any<CancellationToken>())
            .Returns(x =>
            {
                var ids = x.ArgAt<Guid[]>(0);
                return _data
                    .AsQueryable()
                    .Where(p => ids.Contains(p.Id))
                    .ToList();
            });

        var provider = Substitute.For<IServiceProvider>();
        provider
            .GetService(Arg.Any<Type>())
            .Returns(info =>
            {
                if (info.ArgAt<Type>(0) == typeof(IUnitOfWork))
                    return Substitute.For<IUnitOfWork>();
                if (info.ArgAt<Type>(0) == typeof(IEventRepository<EventPayload>))
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
        using var publishWorker = new QueueEventPublishWorker<IEventRepository<EventPayload>, EventPayload>(
            SysClock.Clock,
            _factory,
            _eventBus,
            null,
            TimeSpan.Zero,
            TimeSpan.Zero,
            10,
            true,
            true,
            null
        );


        // Act
        await publishWorker.StartAsync(true);


        // Assert
        SpinWait.SpinUntil(() => _publish.Count == 2, TimeSpan.FromSeconds(5));
        _publish.Count.Should().Be(2);
        _publish.Should().Contain(_data[1]);
        _publish.Should().Contain(_data[2]);
    }
}
