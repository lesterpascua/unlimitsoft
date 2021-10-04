using SoftUnlimit.Web;
using System;
using System.Linq;

namespace SoftUnlimit.EventBus.Azure.Configuration
{
    /// <summary>
    /// 
    /// </summary>
    public class AzureEventBusOptions<TAlias>
        where TAlias : Enum
    {
        /// <summary>
        /// 
        /// </summary>
        public AzureEventBusOptions()
        {
            PublishQueues = Enum
                .GetValues(typeof(TAlias))
                .Cast<TAlias>()
                .Select(s => new QueueAlias<TAlias> { Alias = s, Queue = s.ToPrettyString() })
                .ToArray();
        }

        /// <summary>
        /// Azure endpoint connection string.
        /// </summary>
        public string Endpoint { get; set; }
        /// <summary>
        /// Queue asociate with the current service.
        /// </summary>
        public QueueAlias<TAlias> Queue { get; set; }
        /// <summary>
        /// Queues where this service can publish event.
        /// </summary>
        public QueueAlias<TAlias>[] PublishQueues { get; set; }

        /// <summary>
        /// Activate all queue in the arguments.
        /// </summary>
        /// <param name="onlyNull">If true only activate if the entry has Active in null.</param>
        /// <param name="queues"></param>
        public void ActivateQueues(bool onlyNull, params TAlias[] queues)
        {
            var cache = queues.ToDictionary(k => k);
            foreach (var entry in PublishQueues)
                if ((!onlyNull || entry.Active == null) && cache.ContainsKey(entry.Alias))
                    entry.Active = true;
        }
    }
}
