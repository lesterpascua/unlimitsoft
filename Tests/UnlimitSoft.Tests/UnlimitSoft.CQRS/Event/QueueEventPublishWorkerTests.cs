using Bogus;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.Data;
using UnlimitSoft.CQRS.Data.Dto;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.Data;
using UnlimitSoft.Web.Model;
using Xunit;
using Xunit.Abstractions;

namespace UnlimitSoft.Tests.UnlimitSoft.CQRS.Event;


public sealed class QueueEventPublishWorkerTests
{
    private readonly Faker _faker;
    private readonly ITestOutputHelper _output;

    public QueueEventPublishWorkerTests(ITestOutputHelper output)
    {
        _faker = new Faker();
        _output = output;
    }

    [Theory]
    [InlineData(1_000, 9_999)]
    //[InlineData(1_000_000, 9_999_999)]
    public async Task GenerateHugeAmountOfEvent_AllEventReadyToDeploy_ShouldPublishAllInEventBus(int min, int max)
    {
        // Arrange
        var publish = new List<EventPayload>();
        var collection = Enumerable
            .Range(0, _faker.Random.Int(min, max))
            .Select(i => new EventPayload { Id = _faker.Random.Guid(), Created = _faker.Date.Past(), IsPubliched = false, Scheduled = null })
            .ToDictionary(k => k.Id);

        using var scope = Mock(null, collection, publish, out var eventBus, out var factory);
        using var publishWorker = new QueueEventPublishWorker<IEventRepository<EventPayload>, EventPayload>(
            SysClock.Clock,
            factory,
            eventBus,
            null,
            TimeSpan.Zero,
            TimeSpan.Zero,
            10,
            true,
            null
        );
        await publishWorker.StartAsync(true);

        var sendertask = Task.Run(async () =>
        {
            foreach (var (key, value) in collection.OrderBy(x => x.Value.Created))
                await publishWorker.PublishAsync(new[] { new PublishEventInfo(key, value.Created, value.Scheduled) });
        });


        // Act
        var start = Stopwatch.GetTimestamp();
        SpinWait.SpinUntil(() => publish.Count == collection.Count, TimeSpan.FromMinutes(15));
        _output.WriteLine("Publish {0} after {1} time", publish.Count, Stopwatch.GetElapsedTime(start));

        // Assert
        publish.Count.Should().Be(collection.Count);
        publish.Count(x => x.IsPubliched).Should().Be(collection.Count);
        for (var i = 0; i < publish.Count - 1; i++)
            publish[i].Created.Should().NotBeAfter(publish[i + 1].Created);
    }

    [Fact]
    public async Task StartLoadingPendingEvent_Load3EventOnly2SchedulerInTime_ShouldPublish2EventInEventBus()
    {
        // Arrange
        var publish = new List<EventPayload>();
        var data = new EventPayload[] {
            new EventPayload{ Id = Guid.NewGuid(), Created = new DateTime(1, 12, 31), IsPubliched = true, Scheduled = DateTime.MaxValue },
            new EventPayload{ Id = Guid.NewGuid(), Created = new DateTime(2021, 10, 10), Scheduled = new DateTime(2021, 10, 12) },
            new EventPayload{ Id = Guid.NewGuid(), Created = new DateTime(2021, 10, 11), Scheduled = null },
        };

        using var scope = Mock(data, null, publish, out var eventBus, out var factory);
        using var publishWorker = new QueueEventPublishWorker<IEventRepository<EventPayload>, EventPayload>(
            SysClock.Clock,
            factory,
            eventBus,
            null,
            TimeSpan.Zero,
            TimeSpan.Zero,
            10,
            true,
            null
        );


        // Act
        await publishWorker.StartAsync(true);


        // Assert
        SpinWait.SpinUntil(() => publish.Count == 2, TimeSpan.FromMinutes(5));
        publish.Count.Should().Be(2);
        publish.Should().Contain(data[1]);
        publish.Should().Contain(data[2]);
    }


    #region Private Methods
    private static IServiceScope Mock(EventPayload[]? data, Dictionary<Guid, EventPayload>? generated, List<EventPayload> published, out IEventBus _eventBus, out IServiceScopeFactory factory)
    {
        _eventBus = Substitute.For<IEventBus>();
        _eventBus
            .PublishPayloadAsync(Arg.Any<EventPayload>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(args =>
            {
                published.Add(args.ArgAt<EventPayload>(0));
                return Task.CompletedTask;
            });

        var repository = Substitute.For<IEventRepository<EventPayload>>();
        repository.MarkEventsAsPublishedAsync(Arg.Any<EventPayload>(), Arg.Any<CancellationToken>())
            .Returns(x =>
            {
                var payload = x.ArgAt<EventPayload>(0);
                payload.IsPubliched = true;

                return Task.CompletedTask;
            });
        repository.GetNonPublishedEventsAsync(Arg.Any<Paging>(), Arg.Any<CancellationToken>())
            .Returns(x =>
            {
                var paging = x.ArgAt<Paging>(0);
                if (data is null)
                    return new List<NonPublishEventPayload>();

                return data
                    .Where(p => !p.IsPubliched)
                    .Skip(paging.Page * paging.PageSize)
                    .Take(paging.PageSize)
                    .Select(s => new NonPublishEventPayload(s.Id, s.SourceId, s.Version, s.Created, s.Scheduled))
                    .ToList();
            });
        repository.GetEventsAsync(Arg.Any<Guid[]>(), Arg.Any<CancellationToken>())
            .Returns(x =>
            {
                var result = new List<EventPayload>();
                var ids = x.ArgAt<Guid[]>(0);
                if (data is not null)
                    result.AddRange(data.AsQueryable().Where(p => ids.Contains(p.Id)));
                if (generated is not null)
                    result.AddRange(ids.Where(k => generated.ContainsKey(k)).Select(s => generated[s]));

                return result;
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

        factory = Substitute.For<IServiceScopeFactory>();
        factory.CreateScope().Returns(scope);

        return scope;
    }
    #endregion
}
