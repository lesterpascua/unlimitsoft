namespace UnlimitSoft.Event;


/// <summary>
/// 
/// </summary>
public interface IMementoEvent<TEntity> : IEvent
{
    /// <summary>
    /// Transform the entity acording with the data in the event.
    /// </summary>
    /// <param name="entity"></param>
    void Apply(TEntity entity);
}
