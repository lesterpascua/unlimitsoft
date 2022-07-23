using UnlimitSoft.Web;
using System;
using System.Linq;

namespace UnlimitSoft.EventBus.Azure.Configuration
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
        /// Queues used for the current service to listener messages.
        /// </summary>
        public QueueAlias<TAlias>[] ListenQueues { get; set; }
        /// <summary>
        /// Queues used for the current service to publish messages event.
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
