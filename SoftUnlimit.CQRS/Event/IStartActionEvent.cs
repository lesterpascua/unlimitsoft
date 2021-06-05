using SoftUnlimit.CQRS.Message;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.CQRS.Event
{
    /// <summary>
    /// Indicate this is a start action event in saga process.
    /// </summary>
    [Obsolete]
    public interface IStartActionEvent : IEvent
    {
    }
}
