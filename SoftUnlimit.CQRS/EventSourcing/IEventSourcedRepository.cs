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
        Task<TEntity> FindById(string sourceID, long? version = null);
    }
}
