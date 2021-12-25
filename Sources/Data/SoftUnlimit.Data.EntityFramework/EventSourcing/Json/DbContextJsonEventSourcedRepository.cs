using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SoftUnlimit.CQRS.Query;
using SoftUnlimit.Data;
using SoftUnlimit.Web.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.EventSourcing.Json
{
    /// <summary>
    /// 
    /// </summary>
    public class DbContextJsonEventSourcedRepository : IEventSourcedRepository<JsonVersionedEventPayload, string>
    {
        private readonly ILogger _logger;
        private readonly DbContext _dbContext;
        private readonly DbSet<JsonVersionedEventPayload> _repository;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public DbContextJsonEventSourcedRepository(DbContext dbContext, ILogger logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _repository = _dbContext.Set<JsonVersionedEventPayload>();
        }


        /// <inheritdoc />
        public virtual async Task<NonPublishVersionedEventPayload[]> GetNonPublishedEventsAsync(CancellationToken ct = default)
        {
            return await _repository
                .Where(p => !p.IsPubliched)
                .OrderBy(p => p.Id)
                .Select(s => new NonPublishVersionedEventPayload(s.Id, s.SourceId, s.Version, s.Created, s.Scheduled))
                .ToArrayAsync(ct);
        }
        /// <inheritdoc />
        public virtual async Task<NonPublishVersionedEventPayload[]> GetNonPublishedEventsAsync(Paging paging, CancellationToken ct = default)
        {
            return await _repository
                .Where(p => !p.IsPubliched)
                .OrderBy(p => p.Id)
                .ApplyPagging(paging.Page, paging.PageSize)
                .Select(s => new NonPublishVersionedEventPayload(s.Id, s.SourceId, s.Version, s.Created, s.Scheduled))
                .ToArrayAsync(ct);
        }

        /// <inheritdoc />
        public virtual async Task MarkEventsAsPublishedAsync(JsonVersionedEventPayload @event, CancellationToken ct = default)
        {
            @event.MarkEventAsPublished();
            int amount = await _dbContext.SaveChangesAsync(ct);

            _logger.LogDebug("MarkEventsAsPublishedAsync {Amount} passed {@Event}", amount, @event);
        }
        /// <inheritdoc />
        public virtual async Task MarkEventsAsPublishedAsync(IEnumerable<JsonVersionedEventPayload> events, CancellationToken ct = default)
        {
            foreach (var @event in events)
                @event.MarkEventAsPublished();
            int amount = await _dbContext.SaveChangesAsync(ct);

            _logger.LogDebug("MarkEventsAsPublishedAsync {Amount} passed {@Events}", amount, events);
        }

        /// <inheritdoc />
        public virtual Task<JsonVersionedEventPayload> GetEventAsync(Guid id, CancellationToken ct = default) => _repository.FirstOrDefaultAsync(p => p.Id == id, ct);
        /// <inheritdoc />
        public virtual Task<JsonVersionedEventPayload[]> GetEventsAsync(Guid[] ids, CancellationToken ct = default) => _repository.Where(p => ids.Contains(p.Id)).OrderBy(k => k.Created).ToArrayAsync(ct);

        /// <inheritdoc />
        public virtual async Task<VersionedEntity[]> GetAllSourceIdAsync(Paging page, CancellationToken ct = default)
        {
            var query = from versionedEvent in _repository
                        group versionedEvent by versionedEvent.SourceId into versionedEventGroup
                        select new VersionedEntity(versionedEventGroup.Key, versionedEventGroup.Max(k => k.Version));
            if (page is not null)
                query = query.OrderBy(k => k.Id).ApplyPagging(page.Page, page.PageSize);

            return await query.ToArrayAsync(ct);
        }

        /// <inheritdoc />
        public virtual async Task<JsonVersionedEventPayload> GetAsync(string sourceId, long? version = null, CancellationToken ct = default)
        {
            var query = version.HasValue ? 
                _repository.Where(p => p.SourceId == sourceId && p.Version == version.Value) :
                _repository.Where(p => p.SourceId == sourceId).OrderByDescending(k => k.Version);

            return await query.FirstOrDefaultAsync(ct);
        }
        /// <inheritdoc />
        public virtual async Task<JsonVersionedEventPayload> GetAsync(string sourceId, DateTime? dateTime = null, CancellationToken ct = default)
        {
            var query = dateTime.HasValue ?
                _repository.Where(p => p.SourceId == sourceId && p.Created <= dateTime.Value) :
                _repository.Where(p => p.SourceId == sourceId).OrderByDescending(k => k.Created);

            return await query.FirstOrDefaultAsync(ct);
        }

        /// <inheritdoc />
        public virtual async Task<JsonVersionedEventPayload[]> GetHistoryAsync(string sourceId, long version, CancellationToken ct = default)
        {
            var query = _repository
                .Where(p => p.SourceId == sourceId && p.Version <= version)
                .OrderBy(k => k.Version);

            return await query.ToArrayAsync(ct);
        }
        /// <inheritdoc />
        public virtual async Task<JsonVersionedEventPayload[]> GetHistoryAsync(string sourceId, DateTime dateTime, CancellationToken ct = default)
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
        public virtual async Task<JsonVersionedEventPayload> CreateAsync(JsonVersionedEventPayload eventPayload, bool forceSave = false, CancellationToken ct = default)
        {
            await _repository.AddAsync(eventPayload, ct);
            if (forceSave)
                await _dbContext.SaveChangesAsync(ct);

            _logger.LogDebug("With {Force} CreateAsync {@EventPayload} JsonVersionedEventPayload", forceSave, eventPayload);
            return eventPayload;
        }
        /// <inheritdoc />
        public virtual async Task<IEnumerable<JsonVersionedEventPayload>> CreateAsync(IEnumerable<JsonVersionedEventPayload> eventPayloads, bool forceSave = false, CancellationToken ct = default)
        {
            await _repository.AddRangeAsync(eventPayloads, ct);
            if (forceSave)
                await _dbContext.SaveChangesAsync(ct);

            _logger.LogDebug("With {Force} CreateAsync {@EventPayload} JsonVersionedEventPayload", forceSave, eventPayloads);
            return eventPayloads;
        }
    }
}
