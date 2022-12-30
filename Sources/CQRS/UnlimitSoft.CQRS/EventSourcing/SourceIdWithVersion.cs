using System;

namespace UnlimitSoft.CQRS.EventSourcing;


/// <summary>
/// 
/// </summary>
/// <param name="Id"></param>
/// <param name="Version"></param>
public record SourceIdWithVersion(Guid Id, long Version);
