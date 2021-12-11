using SoftUnlimit.Event;

namespace SoftUnlimit.CQRS.Memento
{
    /// <summary>
    /// 
    /// </summary>
    public interface IMementoEvent<TEntity> : IVersionedEvent
    {
        /// <summary>
        /// Apply the event transformation
        /// </summary>
        /// <param name="entity"></param>
        void Apply(TEntity entity);
        /// <summary>
        /// Rollback the entity.
        /// </summary>
        /// <param name="entity"></param>
        void Rollback(TEntity entity);
    }
}
