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
using UnlimitSoft.Web.Model;

namespace UnlimitSoft.CQRS.Event.Json;


/// <summary>
/// 
/// </summary>
public class JsonEventDbContextRepository : IEventRepository<JsonEventPayload, string>
{
    private readonly DbContext _dbContext;
    private readonly DbSet<JsonEventPayload> _repository;
    private readonly ILogger<JsonEventDbContextRepository> _logger;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="logger"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public JsonEventDbContextRepository(DbContext dbContext, ILogger<JsonEventDbContextRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _repository = _dbContext.Set<JsonEventPayload>();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }


    /// <inheritdoc />
    public virtual async Task<NonPublishEventPayload[]> GetNonPublishedEventsAsync(Paging? paging = null, CancellationToken ct = default)
    {
        IQueryable<JsonEventPayload> query = _repository
            .Where(p => !p.IsPubliched)
            .OrderBy(p => p.Id);
        if (paging is not null)
            query = query.ApplyPagging(paging.Page, paging.PageSize);

        var data = await query
            .Select(s => new NonPublishEventPayload(s.Id, s.SourceId, s.Version, s.Created, s.Scheduled))
            .ToArrayAsync(ct);
        return data ?? Array.Empty<NonPublishEventPayload>();
    }

    /// <inheritdoc />
    public virtual async Task MarkEventsAsPublishedAsync(JsonEventPayload @event, CancellationToken ct = default)
    {
        @event.MarkEventAsPublished();
        _dbContext.Update(@event);
        int amount = await _dbContext.SaveChangesAsync(ct);

        _logger.LogDebug("MarkEventsAsPublishedAsync {Amount} passed {@Event}", amount, @event);
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

        _logger.LogDebug("MarkEventsAsPublishedAsync {Amount} passed {@Events}", amount, events);
    }

    /// <inheritdoc />
    public virtual Task<JsonEventPayload?> GetEventAsync(Guid id, CancellationToken ct = default) => _repository.FirstOrDefaultAsync(p => p.Id == id, ct);
    /// <inheritdoc />
    public virtual Task<JsonEventPayload[]> GetEventsAsync(Guid[] ids, CancellationToken ct = default) => _repository.Where(p => ids.Contains(p.Id)).OrderBy(k => k.Created).ToArrayAsync(ct);

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

    /// <inheritdoc />
    public virtual async Task<JsonEventPayload[]> GetHistoryAsync(Guid sourceId, long version, CancellationToken ct = default)
    {
        var query = _repository
            .Where(p => p.SourceId == sourceId && p.Version <= version)
            .OrderBy(k => k.Version);

        return await query.ToArrayAsync(ct);
    }
    /// <inheritdoc />
    public virtual async Task<JsonEventPayload[]> GetHistoryAsync(Guid sourceId, DateTime dateTime, CancellationToken ct = default)
    {
        var query = _repository
            .Where(p => p.SourceId == sourceId && p.Created <= dateTime)
            .OrderBy(k => k.Version);

        return await query.ToArrayAsync(ct);
    }

    /// <inheritdoc />
    public virtual async Task SavePendingCangesAsync(CancellationToken ct = default)
    {
        int amount = await _dbContext.SaveChangesAsync(ct);
        _logger.LogDebug("SavePendingCangesAsync {Amount} JsonVersionedEventPayload", amount);
    }
    /// <inheritdoc />
    public virtual async ValueTask<JsonEventPayload> CreateAsync(JsonEventPayload eventPayload, bool forceSave = false, CancellationToken ct = default)
    {
        await _repository.AddAsync(eventPayload, ct);
        if (forceSave)
            await _dbContext.SaveChangesAsync(ct);

        _logger.LogDebug("With {Force} CreateAsync {@EventPayload} JsonVersionedEventPayload", forceSave, eventPayload);
        return eventPayload;
    }
    /// <inheritdoc />
    public virtual async Task<IEnumerable<JsonEventPayload>> CreateAsync(IEnumerable<JsonEventPayload> eventPayloads, bool forceSave = false, CancellationToken ct = default)
    {
        await _repository.AddRangeAsync(eventPayloads, ct);
        if (forceSave)
            await _dbContext.SaveChangesAsync(ct);

        _logger.LogDebug("With {Force} CreateAsync {@EventPayload} JsonVersionedEventPayload", forceSave, eventPayloads);
        return eventPayloads;
    }
}
