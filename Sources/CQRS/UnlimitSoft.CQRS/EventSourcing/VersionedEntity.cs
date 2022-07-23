namespace UnlimitSoft.CQRS.EventSourcing
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="Version"></param>
    public record VersionedEntity(string Id, long Version);
}
