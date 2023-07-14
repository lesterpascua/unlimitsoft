using System;

namespace UnlimitSoft.CQRS.Data.Dto;


/// <summary>
/// 
/// </summary>
/// <param name="Id"></param>
/// <param name="Version"></param>
public sealed record SourceIdWithVersion(Guid Id, long Version);
