using System;

namespace UnlimitSoft.CQRS.Data.Dto;


/// <summary>
/// 
/// </summary>
/// <param name="Id"></param>
/// <param name="Version"></param>
public record SourceIdWithVersion(Guid Id, long Version);
