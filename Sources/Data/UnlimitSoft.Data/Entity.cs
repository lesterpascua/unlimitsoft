using System;

namespace UnlimitSoft.Data;


/// <summary>
/// Base entity implementation.
/// </summary>
/// <typeparam name="TKey"></typeparam>f
public abstract class Entity<TKey> : IEntity where TKey : notnull
{
    /// <inheritdoc/>
    public virtual TKey Id { get; set; } = default!;

    /// <inheritdoc/>
    public bool IsTransient()
    {
        if (typeof(TKey) == typeof(long) || typeof(TKey) == typeof(int) || typeof(TKey) == typeof(Guid))
            return Id.Equals(default(TKey));

        return false;
    }
    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (GetType() != obj.GetType())
            return false;
        var item = (Entity<TKey>)obj;
        return item.Id.Equals(Id);
    }

    /// <inheritdoc />
    object IEntity.GetId() => Id;
}
