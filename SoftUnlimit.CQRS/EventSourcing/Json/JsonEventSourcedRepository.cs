using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SoftUnlimit.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.EventSourcing.Json
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class JsonEventSourcedRepository<TEntity> : IEventSourcedRepository<TEntity>
        where TEntity : class, IEventSourced
    {
        private readonly IQueryRepository<JsonVersionedEventPayload> _queryRepository;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryRepository"></param>
        public JsonEventSourcedRepository(IQueryRepository<JsonVersionedEventPayload> queryRepository)
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
            IQueryable<JsonVersionedEventPayload> query = version.HasValue ?
                this._queryRepository.Find(p => p.SourceID.Equals(sourceID) && p.Version == version) :
                this._queryRepository.Find(p => p.SourceID.Equals(sourceID)).OrderByDescending(k => k.Version);

            JsonVersionedEventPayload payload = await query.FirstOrDefaultAsync();
            return JsonConvert.DeserializeObject<TEntity>(payload.CurrState, VersionedEventSettings.JsonDeserializerSettings);
        }
    }
}
