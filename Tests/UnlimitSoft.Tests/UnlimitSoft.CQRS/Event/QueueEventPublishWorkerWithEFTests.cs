﻿using Bogus;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.Data;
using UnlimitSoft.CQRS.Data.Dto;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.CQRS.Event.Json;
using UnlimitSoft.Data.EntityFramework.Utility;
using UnlimitSoft.Message;
using Xunit;

namespace UnlimitSoft.Tests.UnlimitSoft.CQRS.Event;


public sealed class QueueEventPublishWorkerWithEFTests
{
    private readonly Faker _faker;


    public QueueEventPublishWorkerWithEFTests()
    {
        _faker = new Faker();
    }

    [Theory]
    [InlineData(100)]
    [InlineData(10_000)]
    [InlineData(100_000)]
    public async Task StartLoadingPendingEvent_LoadEventSeveralSize_EventShouldBeLoad(int amount)
    {
        using var rootProvider = CreateProvider(TimeSpan.FromHours(1), out var _);
        using var scope = rootProvider.CreateScope();
        var provider = scope.ServiceProvider;

        // Arrange
        var data = new EventPayload[amount];
        for (var i = 0; i < amount; i++)
            data[i] = new EventPayload
            {
                Id = _faker.Random.Guid(),
                Created = _faker.Date.Past(),
                IsPubliched = false,
                Scheduled = null,
                Name = _faker.Random.String2(3),
                Body = """
                        {
                            id: "1",
                            name: "Test"
                        }
                        """
            };

        var dbContext = provider.GetRequiredService<MyDbContext>();
        await dbContext.Events.AddRangeAsync(data);
        await dbContext.SaveChangesAsync();

        var worker = provider.GetRequiredService<IEventPublishWorker>();


        // Act
        await worker.StartAsync(true);


        // Assert
        worker.Pending.Should().Be(amount);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1_000)]
    public async Task StartLoadingPendingEventAndWaitForPublish_LoadEventSeveralSize_EventShouldBePublish(int amount)
    {
        using var rootProvider = CreateProvider(TimeSpan.Zero, out var eventBus);
        using var scope = rootProvider.CreateScope();
        var provider = scope.ServiceProvider;

        // Arrange
        var data = new EventPayload[amount];
        for (var i = 0; i < amount; i++)
            data[i] = new EventPayload
            {
                Id = _faker.Random.Guid(),
                Created = _faker.Date.Past(),
                IsPubliched = false,
                Scheduled = null,
                Name = _faker.Random.String2(3),
                Body = """
                        {
                            id: "1",
                            name: "Test"
                        }
                        """
            };

        var dbContext = provider.GetRequiredService<MyDbContext>();
        await dbContext.Events.AddRangeAsync(data);
        await dbContext.SaveChangesAsync();

        var worker = provider.GetRequiredService<IEventPublishWorker>();


        // Act
        await worker.StartAsync(true);
        SpinWait.SpinUntil(() => worker.Pending == 0);


        // Assert
        worker.Pending.Should().Be(0);
        await eventBus.Received(amount).PublishPayloadAsync(Arg.Any<EventPayload>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    #region Private Methods
    private static ServiceProvider CreateProvider(TimeSpan startDelay, out IEventBus eventBus)
    {
        eventBus = Substitute.For<IEventBus>();
        eventBus
            .PublishPayloadAsync(Arg.Any<EventPayload>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(args => Task.CompletedTask);

        var dbName = Guid.NewGuid().ToString();
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddLogging();
        serviceCollection.AddDbContext<MyDbContext>(opt => opt.UseInMemoryDatabase(dbName));
        serviceCollection.AddSingleton<ISysClock, SysClock>();
        serviceCollection.AddScoped<IEventRepository<EventPayload>, EventDbContextRepository<EventPayload>>();

        serviceCollection.AddSingleton<IEventPublishWorker>(provider =>
        {
            var clock = provider.GetRequiredService<ISysClock>();
            var eventBus = provider.GetRequiredService<IEventBus>();
            var factory = provider.GetRequiredService<IServiceScopeFactory>();
            return new QueueEventPublishWorker<IEventRepository<EventPayload>, EventPayload>(
                clock,
                factory,
                eventBus,
                null,
                startDelay,
                TimeSpan.Zero,
                10,
                true
            );
        });
        serviceCollection.AddSingleton(eventBus);

        return serviceCollection.BuildServiceProvider();
    }
    #endregion

    #region Nested Classes
    private sealed class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new EventEntityTypeBuilder());

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<EventPayload> Events => Set<EventPayload>();
    }
    private sealed class EventEntityTypeBuilder : IEntityTypeConfiguration<EventPayload>
    {
        public void Configure(EntityTypeBuilder<EventPayload> builder)
        {
            EntityBuilderUtility.ConfigureEvent<EventPayload>(builder, useExtraIndex: true);
        }
    }
    #endregion
}
