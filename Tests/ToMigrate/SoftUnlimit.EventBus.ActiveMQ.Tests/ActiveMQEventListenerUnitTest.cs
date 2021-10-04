using Microsoft.Extensions.Logging;
using Moq;
using SoftUnlimit.CQRS.Event;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace SoftUnlimit.EventBus.ActiveMQ.Tests
{
    public class ActiveMQEventListenerUnitTest
    {
        [Fact]
        public void DisposeInvalidConnecion_MustWork()
        {var lister = new ActiveMQEventListener(Guid.NewGuid().ToString(), "TestQueue", "tcp://localhost:61617", (evelop, logger) => Task.CompletedTask);
            lister.Listen(TimeSpan.FromSeconds(1));

            lister.Dispose();
        }
    }
}
