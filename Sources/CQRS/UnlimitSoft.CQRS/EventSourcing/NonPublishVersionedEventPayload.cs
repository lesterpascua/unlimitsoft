using System;

namespace UnlimitSoft.CQRS.EventSourcing
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="SourceId"></param>
    /// <param name="Version"></param>
    /// <param name="Created"></param>
    /// <param name="Scheduled"></param>
    public record NonPublishVersionedEventPayload(Guid Id, string SourceId, long Version, DateTime Created, DateTime? Scheduled);
}
