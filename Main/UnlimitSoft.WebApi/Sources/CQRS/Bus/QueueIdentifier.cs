using UnlimitSoft.EventBus;

namespace UnlimitSoft.WebApi.Sources.CQRS.Bus;


public enum QueueIdentifier
{
    [QueueOrTopicName("account-lester")]
    MyQueue = 1
}
