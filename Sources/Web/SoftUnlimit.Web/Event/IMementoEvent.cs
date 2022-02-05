namespace SoftUnlimit.Event
{
    /// <summary>
    /// 
    /// </summary>
    public interface IMementoEvent<TEntity> : IVersionedEvent
    {
        /// <summary>
        /// Build the entity using the event data. This will use when the event save an snapshot of the entity.
        /// </summary>
        /// <returns></returns>
        TEntity GetEntity();

        /// <summary>
        /// Transform the entity acording with the data in the event.
        /// </summary>
        /// <param name="entity"></param>
        void Apply(TEntity entity);
        /// <summary>
        /// Rollback the change applied to the entity acording with the data in the event. (only support if the event store the previous state)
        /// </summary>
        /// <param name="entity"></param>
        void Rollback(TEntity entity);
    }
}
