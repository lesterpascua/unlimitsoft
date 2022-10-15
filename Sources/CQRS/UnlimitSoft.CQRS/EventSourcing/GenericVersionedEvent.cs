﻿using UnlimitSoft.Event;

namespace UnlimitSoft.CQRS.EventSourcing
{
    /// <summary>
    /// Generic versioned event use to deserialize any kind of event.
    /// </summary>
    public sealed class GenericVersionedEvent : VersionedEvent<object, object>
    {
    }
}