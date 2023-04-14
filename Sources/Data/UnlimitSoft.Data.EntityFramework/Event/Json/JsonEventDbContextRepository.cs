using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.Data;
using UnlimitSoft.CQRS.Data.Dto;
using UnlimitSoft.CQRS.Query;
using UnlimitSoft.Web.Model;

namespace UnlimitSoft.CQRS.Event.Json;


/// <summary>
/// 
/// </summary>
public class JsonEventDbContextRepository : IEventRepository<JsonEventPayload, string>
{
    private readonly DbContext _dbContext;
    private readonly DbSet<JsonEventPayload> _repository;
    private readonly ILogger<JsonEventDbContextRepository>? _logger;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="logger"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public JsonEventDbContextRepository(DbContext dbContext, ILogger<JsonEventDbContextRepository>? logger = null)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _repository = _dbContext.Set<JsonEventPayload>();
        _logger = logger;
    }


    private static Func<DbContext, IAsyncEnumerable<NonPublishEventPayload>>? _getNonPublishedEventsWithOutPaging;
    private static Func<DbContext, int, int, IAsyncEnumerable<NonPublishEventPayload>>? _getNonPublishedEventsWithPaging;
    /// <inheritdoc />
    public virtual async Task<List<NonPublishEventPayload>> GetNonPublishedEventsAsync(Paging? paging = null, CancellationToken ct = default)
    {
        if (paging is not null)
        {
            if (_getNonPublishedEventsWithPaging is null)
                Interlocked.CompareExchange(
                    ref _getNonPublishedEventsWithPaging,
                    EF.CompileAsyncQuery(
                        (DbContext dbContext, int page, int size) => dbContext.Set<JsonEventPayload>()
                            .Where(x => !x.IsPubliched)
                            .OrderBy(x => x.Id)
                            .Skip(page * size)
                            .Take(size)
                            .Select(s => new NonPublishEventPayload(s.Id, s.SourceId, s.Version, s.Created, s.Scheduled))
                    ),
                    null
                );
            await using var withPagingEnum = _getNonPublishedEventsWithPaging(_dbContext, paging.Page, paging.PageSize).GetAsyncEnumerator(ct);

            var withPaging = new List<NonPublishEventPayload>();
            while (await withPagingEnum.MoveNextAsync())
                withPaging.Add(withPagingEnum.Current);
            return withPaging;
        }

        if (_getNonPublishedEventsWithOutPaging is null)
            Interlocked.CompareExchange(
                ref _getNonPublishedEventsWithOutPaging,
                EF.CompileAsyncQuery(
                    (DbContext dbContext) => dbContext.Set<JsonEventPayload>()
                        .Where(x => !x.IsPubliched)
                        .OrderBy(x => x.Id)
                        .Select(s => new NonPublishEventPayload(s.Id, s.SourceId, s.Version, s.Created, s.Scheduled))
                ),
                null
            );
        await using var withOutPagingEnum = _getNonPublishedEventsWithOutPaging(_dbContext).GetAsyncEnumerator(ct);

        var withOutPaging = new List<NonPublishEventPayload>();
        while (await withOutPagingEnum.MoveNextAsync())
            withOutPaging.Add(withOutPagingEnum.Current);
        return withOutPaging;
    }

    /// <inheritdoc />
    public virtual async Task MarkEventsAsPublishedAsync(JsonEventPayload @event, CancellationToken ct = default)
    {
        @event.MarkEventAsPublished();
        _dbContext.Update(@event);
        int amount = await _dbContext.SaveChangesAsync(ct);

        _logger?.LogDebug("MarkEventsAsPublishedAsync {Amount} passed {@Event}", amount, @event);
    }
    /// <inheritdoc />
    public virtual async Task MarkEventsAsPublishedAsync(IEnumerable<JsonEventPayload> events, CancellationToken ct = default)
    {
        foreach (var @event in events)
        {
            @event.MarkEventAsPublished();
            _dbContext.Update(@event);
        }
        int amount = await _dbContext.SaveChangesAsync(ct);

        _logger?.LogDebug("MarkEventsAsPublishedAsync {Amount} passed {@Events}", amount, events);
    }

    private Func<DbContext, Guid, Task<JsonEventPayload?>>? _getEvent;
    /// <inheritdoc />
    public virtual Task<JsonEventPayload?> GetEventAsync(Guid id, CancellationToken ct = default)
    {
        if (_getEvent is null)
            Interlocked.CompareExchange(
                ref _getEvent,
                EF.CompileAsyncQuery(
                    (DbContext dbContext, Guid id) => dbContext.Set<JsonEventPayload>().FirstOrDefault(x => x.Id == id)
                ),
                null
            );
        return _getEvent(_dbContext, id);
    }

    private static Func<DbContext, Guid[], IAsyncEnumerable<JsonEventPayload>>? _getEvents;
    /// <inheritdoc />
    public virtual async Task<List<JsonEventPayload>> GetEventsAsync(Guid[] ids, CancellationToken ct = default)
    {
        if (_getEvents is null)
            Interlocked.CompareExchange(
                ref _getEvents,
                EF.CompileAsyncQuery(
                    (DbContext dbContext, Guid[] ids) => dbContext.Set<JsonEventPayload>()
                        .Where(p => ids.Contains(p.Id))
                        .OrderBy(k => k.Created)
                        .AsQueryable()
                ),
                null
            );

        var list = new List<JsonEventPayload>();
        await foreach (var item in _getEvents(_dbContext, ids))
            list.Add(item);

        return list;
    }

    /// <inheritdoc />
    public virtual async Task<SourceIdWithVersion[]> GetAllSourceIdAsync(Paging? page, CancellationToken ct = default)
    {
        var query = from versionedEvent in _repository
                    group versionedEvent by versionedEvent.SourceId into versionedEventGroup
                    select new SourceIdWithVersion(versionedEventGroup.Key, versionedEventGroup.Max(k => k.Version));
        if (page is not null)
            query = query.OrderBy(k => k.Id).ApplyPagging(page.Page, page.PageSize);

        return await query.ToArrayAsync(ct);
    }

    /// <inheritdoc />
    public virtual async Task<JsonEventPayload?> GetAsync(Guid sourceId, long? version = null, CancellationToken ct = default)
    {
        var query = version.HasValue ? 
            _repository.Where(p => p.SourceId == sourceId && p.Version == version.Value) :
            _repository.Where(p => p.SourceId == sourceId).OrderByDescending(k => k.Version);

        return await query.FirstOrDefaultAsync(ct);
    }
    /// <inheritdoc />
    public virtual async Task<JsonEventPayload?> GetAsync(Guid sourceId, DateTime? dateTime = null, CancellationToken ct = default)
    {
        var query = dateTime.HasValue ?
            _repository.Where(p => p.SourceId == sourceId && p.Created <= dateTime.Value) :
            _repository.Where(p => p.SourceId == sourceId).OrderByDescending(k => k.Created);

        return await query.FirstOrDefaultAsync(ct);
    }

    private static Func<DbContext, Guid, long, IAsyncEnumerable<JsonEventPayload>> _getHistoryByVersion = default!;
    /// <inheritdoc />
    public virtual async Task<List<JsonEventPayload>> GetHistoryAsync(Guid sourceId, long version, CancellationToken ct = default)
    {
        if (_getHistoryByVersion is null)
            Interlocked.CompareExchange(
                ref _getHistoryByVersion,
                EF.CompileAsyncQuery((DbContext dbContext, Guid sourceId, long version) => dbContext.Set<JsonEventPayload>().Where(p => p.SourceId == sourceId && p.Version <= version).OrderBy(k => k.Version).AsQueryable()),
                null
            );

        var list = new List<JsonEventPayload>();
        await foreach (var item in _getHistoryByVersion(_dbContext, sourceId, version))
            list.Add(item);
        return list;
    }

    private static Func<DbContext, Guid, DateTime, IAsyncEnumerable<JsonEventPayload>>? _getHistoryByCreate;
    /// <inheritdoc />
    public virtual async Task<List<JsonEventPayload>> GetHistoryAsync(Guid sourceId, DateTime dateTime, CancellationToken ct = default)
    {
        if (_getHistoryByCreate is null)
            Interlocked.CompareExchange(
                ref _getHistoryByCreate,
                EF.CompileAsyncQuery((DbContext dbContext, Guid sourceId, DateTime dateTime) => dbContext.Set<JsonEventPayload>().Where(p => p.SourceId == sourceId && p.Created <= dateTime).OrderBy(k => k.Version).AsQueryable()),
                null
            );

        var list = new List<JsonEventPayload>();
        await foreach (var item in _getHistoryByCreate(_dbContext, sourceId, dateTime))
            list.Add(item);
        return list;
    }

    /// <inheritdoc />
    public virtual async Task SavePendingCangesAsync(CancellationToken ct = default)
    {
        int amount = await _dbContext.SaveChangesAsync(ct);
        _logger?.LogDebug("SavePendingCangesAsync {Amount} JsonVersionedEventPayload", amount);
    }
    /// <inheritdoc />
    public virtual async ValueTask<JsonEventPayload> CreateAsync(JsonEventPayload eventPayload, bool forceSave = false, CancellationToken ct = default)
    {
        await _repository.AddAsync(eventPayload, ct);
        if (forceSave)
            await _dbContext.SaveChangesAsync(ct);

        _logger?.LogDebug("With {Force} CreateAsync {@EventPayload} JsonVersionedEventPayload", forceSave, eventPayload);
        return eventPayload;
    }
    /// <inheritdoc />
    public virtual async Task<IEnumerable<JsonEventPayload>> CreateAsync(IEnumerable<JsonEventPayload> eventPayloads, bool forceSave = false, CancellationToken ct = default)
    {
        await _repository.AddRangeAsync(eventPayloads, ct);
        if (forceSave)
            await _dbContext.SaveChangesAsync(ct);

        _logger?.LogDebug("With {Force} CreateAsync {@EventPayload} JsonVersionedEventPayload", forceSave, eventPayloads);
        return eventPayloads;
    }
}
