using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.Data;
using UnlimitSoft.CQRS.Data.Dto;
using UnlimitSoft.CQRS.Query;
using UnlimitSoft.Json;
using UnlimitSoft.Web.Model;

namespace UnlimitSoft.CQRS.Event.Json;


/// <summary>
/// 
/// </summary>
public class EventDbContextRepository<TEventPayload> : IEventRepository<TEventPayload> where TEventPayload : EventPayload
{
    private readonly bool _optimize;
    private readonly IJsonSerializer _serializer;
    private readonly DbContext _dbContext;
    private readonly DbSet<TEventPayload> _repository;
    private readonly ILogger<EventDbContextRepository<TEventPayload>>? _logger;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="serializer"></param>
    /// <param name="dbContext"></param>
    /// <param name="optimize">Precompile the query and reuse in all iterations</param>
    /// <param name="logger"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public EventDbContextRepository(IJsonSerializer serializer, DbContext dbContext, bool optimize = true, ILogger<EventDbContextRepository<TEventPayload>>? logger = null)
    {
        _optimize = optimize;
        _serializer = serializer;
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _repository = _dbContext.Set<TEventPayload>();
        _logger = logger;
    }

    /// <inheritdoc />
    public IJsonSerializer Serializer => _serializer;

    /// <inheritdoc />
    public virtual async Task MarkEventsAsPublishedAsync(TEventPayload @event, CancellationToken ct = default)
    {
        @event.MarkEventAsPublished();
        _dbContext.Update(@event);
        int amount = await _dbContext.SaveChangesAsync(ct);

        _logger?.LogDebug("MarkEventsAsPublishedAsync {Amount} passed {@Event}", amount, @event);
    }
    /// <inheritdoc />
    public virtual async Task MarkEventsAsPublishedAsync(IEnumerable<TEventPayload> events, CancellationToken ct = default)
    {
        foreach (var @event in events)
        {
            @event.MarkEventAsPublished();
            _dbContext.Update(@event);
        }
        int amount = await _dbContext.SaveChangesAsync(ct);

        _logger?.LogDebug("MarkEventsAsPublishedAsync {Amount} passed {@Events}", amount, events);
    }

    /// <inheritdoc />
    public virtual async Task<List<NonPublishEventPayload>> GetNonPublishedEventsAsync(Paging? paging = null, CancellationToken ct = default)
    {
        if (_optimize)
        {
            var enumerable = GetNonPublishedEventsEnumerable(paging);
            await using var withPagingEnum = enumerable.GetAsyncEnumerator(ct);

            var list = new List<NonPublishEventPayload>();
            while (await withPagingEnum.MoveNextAsync())
                list.Add(withPagingEnum.Current);
            return list;
        }

        IQueryable<TEventPayload> query = _repository
            .Where(p => !p.IsPubliched)
            .OrderBy(p => p.Id);
        if (paging is not null)
            query = query.ApplyPagging(paging.Page, paging.PageSize);

        return await query
            .Select(s => new NonPublishEventPayload(s.Id, s.SourceId, s.Version, s.Created, s.Scheduled))
            .ToListAsync(ct);
    }

    private Func<DbContext, Guid, Task<TEventPayload?>>? _getEvent;
    /// <inheritdoc />
    public virtual Task<TEventPayload?> GetEventAsync(Guid id, CancellationToken ct = default)
    {
        if (_optimize)
        {
            if (_getEvent is null)
                Interlocked.CompareExchange(
                    ref _getEvent,
                    EF.CompileAsyncQuery(
                        (DbContext dbContext, Guid id) => dbContext.Set<TEventPayload>().FirstOrDefault(x => x.Id == id)
                    ),
                    null
                );
            return _getEvent(_dbContext, id);
        }
        return _dbContext.Set<TEventPayload>().FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    private static Func<DbContext, Guid[], IAsyncEnumerable<TEventPayload>>? _getEvents;
    /// <inheritdoc />
    public virtual async Task<List<TEventPayload>> GetEventsAsync(Guid[] ids, CancellationToken ct = default)
    {
        if (_getEvents is null)
            Interlocked.CompareExchange(
                ref _getEvents,
                EF.CompileAsyncQuery(
                    (DbContext dbContext, Guid[] ids) => dbContext.Set<TEventPayload>()
                        .Where(p => ids.Contains(p.Id))
                        .OrderBy(k => k.Created)
                        .AsQueryable()
                ),
                null
            );

        var list = new List<TEventPayload>();
        await foreach (var item in _getEvents(_dbContext, ids))
            list.Add(item);

        return list;
    }

    /// <inheritdoc />
    public virtual async Task<List<SourceIdWithVersion>> GetAllSourceIdAsync(Paging? paging, CancellationToken ct = default)
    {
        if (_optimize)
        {
            var enumerable = GetAllSourceIdEnumerable(paging);

            var list = new List<SourceIdWithVersion>();
            await foreach (var item in enumerable)
                list.Add(item);
            return list;
        }

        var query = from versionedEvent in _repository
                    group versionedEvent by versionedEvent.SourceId into versionedEventGroup
                    select new { Id = versionedEventGroup.Key, Version = versionedEventGroup.Max(k => k.Version) };
        if (paging is not null)
            query = query.OrderBy(k => k.Id).ApplyPagging(paging.Page, paging.PageSize);

        return await query.Select(s => new SourceIdWithVersion(s.Id, s.Version)).ToListAsync(ct);
    }

    private Func<DbContext, Guid, Task<TEventPayload?>>? _getWithOutVersion;
    private Func<DbContext, Guid, long, Task<TEventPayload?>>? _getWithVersion;
    /// <inheritdoc />
    public virtual Task<TEventPayload?> GetAsync(Guid sourceId, long? version = null, CancellationToken ct = default)
    {
        if (_optimize)
        {
            if (version is null)
            {
                if (_getWithOutVersion is null)
                    Interlocked.CompareExchange(
                        ref _getWithOutVersion,
                        EF.CompileAsyncQuery(
                            (DbContext dbContext, Guid sourceId) => dbContext.Set<TEventPayload>().Where(x => x.SourceId == sourceId).OrderByDescending(k => k.Version).FirstOrDefault()
                        ),
                        null
                    );
                return _getWithOutVersion(_dbContext, sourceId);
            }

            if (_getWithVersion is null)
                Interlocked.CompareExchange(
                    ref _getWithVersion,
                    EF.CompileAsyncQuery(
                        (DbContext dbContext, Guid sourceId, long version) => dbContext.Set<TEventPayload>().FirstOrDefault(x => x.SourceId == sourceId && x.Version == version)
                    ),
                    null
                );
            return _getWithVersion(_dbContext, sourceId, version.Value);
        }

        var query = version.HasValue ? 
            _repository.Where(p => p.SourceId == sourceId && p.Version == version.Value) :
            _repository.Where(p => p.SourceId == sourceId).OrderByDescending(k => k.Version);

        return query.FirstOrDefaultAsync(ct);
    }

    private Func<DbContext, Guid, Task<TEventPayload?>>? _getWithOutDate;
    private Func<DbContext, Guid, DateTime, Task<TEventPayload?>>? _getWithDate;
    /// <inheritdoc />
    public virtual Task<TEventPayload?> GetAsync(Guid sourceId, DateTime? dateTime = null, CancellationToken ct = default)
    {
        if (_optimize)
        {
            if (dateTime is null)
            {
                if (_getWithOutDate is null)
                    Interlocked.CompareExchange(
                        ref _getWithOutDate,
                        EF.CompileAsyncQuery(
                            (DbContext dbContext, Guid sourceId) => dbContext.Set<TEventPayload>().Where(x => x.SourceId == sourceId).OrderByDescending(k => k.Created).FirstOrDefault()
                        ),
                        null
                    );
                return _getWithOutDate(_dbContext, sourceId);
            }

            if (_getWithDate is null)
                Interlocked.CompareExchange(
                    ref _getWithDate,
                    EF.CompileAsyncQuery(
                        (DbContext dbContext, Guid sourceId, DateTime created) => dbContext.Set<TEventPayload>().FirstOrDefault(x => x.SourceId == sourceId && x.Created <= created)
                    ),
                    null
                );
            return _getWithDate(_dbContext, sourceId, dateTime.Value);
        }

        var query = dateTime.HasValue ?
            _repository.Where(p => p.SourceId == sourceId && p.Created <= dateTime.Value) :
            _repository.Where(p => p.SourceId == sourceId).OrderByDescending(k => k.Created);

        return query.FirstOrDefaultAsync(ct);
    }

    private static Func<DbContext, Guid, long, IAsyncEnumerable<TEventPayload>> _getHistoryByVersion = default!;
    /// <inheritdoc />
    public virtual async Task<List<TEventPayload>> GetHistoryAsync(Guid sourceId, long version, CancellationToken ct = default)
    {
        if (_getHistoryByVersion is null)
            Interlocked.CompareExchange(
                ref _getHistoryByVersion,
                EF.CompileAsyncQuery((DbContext dbContext, Guid sourceId, long version) => dbContext.Set<TEventPayload>().Where(p => p.SourceId == sourceId && p.Version <= version).OrderBy(k => k.Version).AsQueryable()),
                null
            );

        var list = new List<TEventPayload>();
        await foreach (var item in _getHistoryByVersion(_dbContext, sourceId, version))
            list.Add(item);
        return list;
    }

    private static Func<DbContext, Guid, DateTime, IAsyncEnumerable<TEventPayload>>? _getHistoryByCreate;
    /// <inheritdoc />
    public virtual async Task<List<TEventPayload>> GetHistoryAsync(Guid sourceId, DateTime dateTime, CancellationToken ct = default)
    {
        if (_getHistoryByCreate is null)
            Interlocked.CompareExchange(
                ref _getHistoryByCreate,
                EF.CompileAsyncQuery((DbContext dbContext, Guid sourceId, DateTime dateTime) => dbContext.Set<TEventPayload>().Where(p => p.SourceId == sourceId && p.Created <= dateTime).OrderBy(k => k.Version).AsQueryable()),
                null
            );

        var list = new List<TEventPayload>();
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
    public virtual async ValueTask<TEventPayload> CreateAsync(TEventPayload eventPayload, bool forceSave = false, CancellationToken ct = default)
    {
        await _repository.AddAsync(eventPayload, ct);
        if (forceSave)
            await _dbContext.SaveChangesAsync(ct);

        _logger?.LogDebug("With {Force} CreateAsync {@EventPayload} JsonVersionedEventPayload", forceSave, eventPayload);
        return eventPayload;
    }
    /// <inheritdoc />
    public virtual async Task<IEnumerable<TEventPayload>> CreateAsync(IEnumerable<TEventPayload> eventPayloads, bool forceSave = false, CancellationToken ct = default)
    {
        await _repository.AddRangeAsync(eventPayloads, ct);
        if (forceSave)
            await _dbContext.SaveChangesAsync(ct);

        _logger?.LogDebug("With {Force} CreateAsync {@EventPayload} JsonVersionedEventPayload", forceSave, eventPayloads);
        return eventPayloads;
    }

    #region Private Methods
    private static Func<DbContext, IAsyncEnumerable<NonPublishEventPayload>>? _getNonPublishedEventsWithOutPaging;
    private static Func<DbContext, int, int, IAsyncEnumerable<NonPublishEventPayload>>? _getNonPublishedEventsWithPaging;
    private IAsyncEnumerable<NonPublishEventPayload> GetNonPublishedEventsEnumerable(Paging? paging)
    {
        if (paging is not null)
        {
            if (_getNonPublishedEventsWithPaging is null)
                Interlocked.CompareExchange(
                    ref _getNonPublishedEventsWithPaging,
                    EF.CompileAsyncQuery(
                        (DbContext dbContext, int page, int size) => dbContext.Set<TEventPayload>()
                            .Where(x => !x.IsPubliched)
                            .OrderBy(x => x.Id)
                            .Skip(page * size)
                            .Take(size)
                            .Select(s => new NonPublishEventPayload(s.Id, s.SourceId, s.Version, s.Created, s.Scheduled))
                    ),
                    null
                );
            return _getNonPublishedEventsWithPaging(_dbContext, paging.Page, paging.PageSize);
        }

        if (_getNonPublishedEventsWithOutPaging is null)
            Interlocked.CompareExchange(
                ref _getNonPublishedEventsWithOutPaging,
                EF.CompileAsyncQuery(
                    (DbContext dbContext) => dbContext.Set<TEventPayload>()
                        .Where(x => !x.IsPubliched)
                        .Select(s => new NonPublishEventPayload(s.Id, s.SourceId, s.Version, s.Created, s.Scheduled))
                ),
                null
            );
        return _getNonPublishedEventsWithOutPaging(_dbContext);
    }

    private static Func<DbContext, IAsyncEnumerable<SourceIdWithVersion>>? _getAllSourceIdWithOutPaging;
    private static Func<DbContext, int, int, IAsyncEnumerable<SourceIdWithVersion>>? _getAllSourceIdWithPaging;
    private IAsyncEnumerable<SourceIdWithVersion> GetAllSourceIdEnumerable(Paging? paging)
    {
        if (paging is not null)
        {
            if (_getAllSourceIdWithPaging is null)
                Interlocked.CompareExchange(
                    ref _getAllSourceIdWithPaging,
                    EF.CompileAsyncQuery(
                        (DbContext dbContext, int page, int size) => dbContext.Set<TEventPayload>()
                            .GroupBy(x => x.SourceId)
                            .OrderBy(x => x.Key)
                            .Skip(page * size)
                            .Take(size)
                            .Select(s => new SourceIdWithVersion(s.Key, s.Max(x => x.Version)))
                    ),
                null
                );
            return _getAllSourceIdWithPaging(_dbContext, paging.Page, paging.PageSize);
        }

        if (_getAllSourceIdWithOutPaging is null)
            Interlocked.CompareExchange(
                ref _getAllSourceIdWithOutPaging,
                EF.CompileAsyncQuery(
                    (DbContext dbContext) => dbContext.Set<TEventPayload>()
                        .GroupBy(x => x.SourceId)
                        .Select(s => new SourceIdWithVersion(s.Key, s.Max(x => x.Version)))
                ),
                null
            );
        return _getAllSourceIdWithOutPaging(_dbContext);
    }
    #endregion
}
