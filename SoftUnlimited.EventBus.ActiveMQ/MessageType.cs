using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimited.EventBus.ActiveMQ
{
    public enum MessageType
    {
        Event = 1, 
        Json = 2,
        Binary = 3
    }
}
