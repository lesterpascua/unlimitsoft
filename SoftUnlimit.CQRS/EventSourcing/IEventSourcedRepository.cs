using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SoftUnlimit.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.EventSourcing
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IEventSourcedRepository<TEntity>
        where TEntity : class, IEventSourced
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceID"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        Task<TEntity> FindByID(string sourceID, long? version = null);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class EventSourcedRepository<TEntity> : IEventSourcedRepository<TEntity>
        where TEntity : class, IEventSourced
    {
        private readonly IQueryRepository<VersionedEventPayload> _queryRepository;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryRepository"></param>
        public EventSourcedRepository(IQueryRepository<VersionedEventPayload> queryRepository)
        {
            this._queryRepository = queryRepository;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceID"></param>
        /// <param name="version">if null get the last version.</param>
        /// <returns></returns>
        public async Task<TEntity> FindByID(string sourceID, long? version = null)
        {
            IQueryable<VersionedEventPayload> query = version.HasValue ?
                this._queryRepository.Find(p => p.SourceID.Equals(sourceID) && p.Version == version) :
                this._queryRepository.Find(p => p.SourceID.Equals(sourceID)).OrderByDescending(k => k.Version);

            VersionedEventPayload payload = await query.FirstOrDefaultAsync();
            return JsonConvert.DeserializeObject<TEntity>(payload.CurrState, VersionedEventSettings.JsonDeserializerSettings);
        }
    }
}
