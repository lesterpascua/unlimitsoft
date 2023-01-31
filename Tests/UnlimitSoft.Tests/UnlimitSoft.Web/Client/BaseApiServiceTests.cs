using FluentAssertions;
using LazyCache;
using LazyCache.Providers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using UnlimitSoft.Web.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace UnlimitSoft.Tests.UnlimitSoft.Web.Client;


public class BaseApiServiceTests
{
    [Fact]
    public async Task LazyCache_With50ConcurrencyCall_MakeOnlyOneCallToService()
    {
        // Arrange
        var values = new List<int>();
        var service = new MyApiService(GetLazyCachingObject());

        // Act
        await Parallel.ForEachAsync(Enumerable.Range(1, 50), async (_, ct) =>
        {
            var result = await service.GetAsync();
            values.Add(result);
        });

        // Assert
        service.Counter.Should().Be(1);
        values.All(x => x == 1).Should().BeTrue();
    }
    [Fact]
    public async Task MemoryCache_OneCallToWarmServiceThem50ConcurrencyCall_MakeOnlyOneCallToService()
    {
        // Arrange
        var values = new List<int>();
        var service = new MyApiService(GetMemoryCachingObject());

        // Act
        //var result = await service.GetAsync();
        await Parallel.ForEachAsync(
            Enumerable.Range(1, 50),
            async (_, ct) =>
            {
                var result = await service.GetAsync();
                values.Add(result);
            }
        );

        // Assert
        service.Counter.Should().NotBe(1);
        values.All(x => x == 1).Should().BeFalse();
    }

    #region Private Methods
    private static ICache GetLazyCachingObject()
    {
        var cachingService = new CachingService(
            new Lazy<ICacheProvider>(() =>
            {
                var options = new MemoryCacheOptions();
                var cache = new MemoryCache(new OptionsWrapper<MemoryCacheOptions>(options));
                return new MemoryCacheProvider(cache);
            })
        );
        return new LazyCacheWrapper(cachingService, TimeSpan.FromMinutes(1));
    }
    private static ICache GetMemoryCachingObject()
    {
        var options = new MemoryCacheOptions();
        var cache = new MemoryCache(new OptionsWrapper<MemoryCacheOptions>(options));
        return new MemoryCacheWrapper(cache, TimeSpan.FromMinutes(1));
    }
    #endregion

    #region Nested Classes
    private sealed class LazyCacheWrapper : ICache
    {
        private readonly CachingService _cachingService;
        private readonly TimeSpan _expiration;

        public LazyCacheWrapper(CachingService cachingService, TimeSpan expiration)
        {
            _cachingService = cachingService;
            _expiration = expiration;
        }

        public async ValueTask<TResult?> GetOrCreateAsync<TResult>(object key, Func<object, Task<TResult>> factory)
        {
            return await _cachingService.GetOrAddAsync(key.ToString(), cacheKey =>
            {
                cacheKey.AbsoluteExpirationRelativeToNow = _expiration;
                return factory(cacheKey);
            });
        }
    }
    private sealed class MemoryCacheWrapper : ICache
    {
        private readonly MemoryCache _cachingService;
        private readonly TimeSpan _expiration;

        public MemoryCacheWrapper(MemoryCache cachingService, TimeSpan expiration)
        {
            _cachingService = cachingService;
            _expiration = expiration;
        }

        public async ValueTask<TResult?> GetOrCreateAsync<TResult>(object key, Func<object, Task<TResult>> factory)
        {
            return await _cachingService.GetOrCreateAsync(key, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = _expiration;
                return factory(entry);
            });
        }
    }
    private sealed class MyApiService : BaseApiService
    {
        private int _counter;

        public MyApiService(ICache cache) : base(null, cache)
        {
            _counter = 0;
        }

        public int Counter => _counter;

        public async ValueTask<int> GetAsync()
        {
            return await TryCacheFirst(_ => Task.FromResult(Interlocked.Increment(ref _counter)));
        }
    }
    #endregion
}
