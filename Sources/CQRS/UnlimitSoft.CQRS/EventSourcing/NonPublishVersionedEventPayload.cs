using System;

namespace UnlimitSoft.CQRS.EventSourcing;


/// <summary>
/// Dto used to storage the representation of a non publish event in memory. The event is save in a persitance store
/// and recover only when is necesary publish
/// </summary>
/// <param name="Id"></param>
/// <param name="SourceId"></param>
/// <param name="Version"></param>
/// <param name="Created"></param>
/// <param name="Scheduled"></param>
public record NonPublishVersionedEventPayload(Guid Id, Guid SourceId, long Version, DateTime Created, DateTime? Scheduled);
