using UnlimitSoft.EventBus;

namespace UnlimitSoft.WebApi.EventBus.EventBus;


public enum QueueIdentifier
{
    [QueueOrTopicName("test-name")]
    Test = 1,
    TestService1 = 2,
    TestService2 = 3,
}
