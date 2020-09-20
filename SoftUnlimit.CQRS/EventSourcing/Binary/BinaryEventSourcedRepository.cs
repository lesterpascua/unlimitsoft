using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SoftUnlimit.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.EventSourcing.Binary
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class BinaryEventSourcedRepository<TEntity> : IEventSourcedRepository<TEntity>
        where TEntity : class, IEventSourced
    {
        private readonly IQueryRepository<BinaryVersionedEventPayload> _queryRepository;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryRepository"></param>
        public BinaryEventSourcedRepository(IQueryRepository<BinaryVersionedEventPayload> queryRepository)
        {
            this._queryRepository = queryRepository;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceId"></param>
        /// <param name="version">if null get the last version.</param>
        /// <returns></returns>
        public async Task<TEntity> FindByID(string sourceId, long? version = null)
        {
            IQueryable<BinaryVersionedEventPayload> query = version.HasValue ?
                this._queryRepository.Find(p => p.SourceId.Equals(sourceId) && p.Version == version) :
                this._queryRepository.Find(p => p.SourceId.Equals(sourceId)).OrderByDescending(k => k.Version);

            BinaryVersionedEventPayload payload = await query.FirstOrDefaultAsync();
            var formatter = new BinaryFormatter();
            using var stream = new MemoryStream(payload.RawData);

            return (TEntity)formatter.Deserialize(stream);
        }
    }
}
