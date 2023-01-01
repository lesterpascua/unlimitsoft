using System;
using System.Linq;

namespace UnlimitSoft.EventBus.Configuration;


/// <summary>
/// 
/// </summary>
public abstract class EventBusOptions<TAlias>
    where TAlias : struct, Enum
{
    /// <summary>
    /// Queues used for the current service to listener messages.
    /// </summary>
    public QueueAlias<TAlias>[] ListenQueues { get; set; } = Array.Empty<QueueAlias<TAlias>>();
    /// <summary>
    /// Queues or topic used for the current service to publish messages event.
    /// </summary>
    public QueueAlias<TAlias>[] PublishQueues { get; set; } = Array.Empty<QueueAlias<TAlias>>();

    /// <summary>
    /// Activate all specified alias in the arguments for the listen queues.
    /// </summary>
    /// <param name="onlyNull">If true only activate if the entry has Active in null.</param>
    /// <param name="queues"></param>
    public void ActivateListenAlias(bool onlyNull, params TAlias[] queues) => ListenQueues = Activate(ListenQueues, onlyNull, queues);
    /// <summary>
    /// Activate all specified alias in the arguments for the publish queues.
    /// </summary>
    /// <param name="onlyNull">If true only activate if the entry has Active in null.</param>
    /// <param name="queues"></param>
    public void ActivatePublishAlias(bool onlyNull, params TAlias[] queues) => PublishQueues = Activate(PublishQueues, onlyNull, queues);

    #region Private Methods
    private string GetQueueName(TAlias alias)
    {
        var name = alias.ToString();
        var memberInfo = alias.GetType().GetMember(name);
        if (memberInfo?.Any() == true)
        {
            var attrs = memberInfo[0].GetCustomAttributes(typeof(QueueOrTopicNameAttribute), false);
            if (attrs?.Any() == true && attrs[0] is QueueOrTopicNameAttribute queueOrTopic)
                name = queueOrTopic.Name;
        }
        return name;
    }
    private QueueAlias<TAlias>[] Activate(QueueAlias<TAlias>[] data, bool onlyNull, TAlias[] queues)
    {
        // Cache current state
        var list = data.ToList();
        foreach (var alias in queues)
        {
            var aux = list.Find(x => x.Alias.Equals(alias));
            if (onlyNull && aux?.Active is not null)
                continue;

            if (aux is null)
            {
                aux = new QueueAlias<TAlias> { Active = true, Alias = alias, Queue = GetQueueName(alias) };
                list.Add(aux);
                continue;
            }
            aux.Active = true;
        }
        return list.ToArray();
    }
    #endregion
}
