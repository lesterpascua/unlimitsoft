using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Event
{
    /// <summary>
    /// Receive an event and verify if can proceess.
    /// </summary>
    public interface IEventReceiptProcessor
    {
        /// <summary>
        /// Check if event is valid and the serialization, convert to event object and dispatch.
        /// </summary>
        /// <param name="envelop"></param>
        /// <returns></returns>
        Task<bool> Process(MessageEnvelop envelop);
    }
}
