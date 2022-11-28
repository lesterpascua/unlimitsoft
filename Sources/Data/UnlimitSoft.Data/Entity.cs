using System;

namespace UnlimitSoft.Data;


/// <summary>
/// Base entity implementation.
/// </summary>
/// <typeparam name="Key"></typeparam>f
public abstract class Entity<Key> : IEntity 
    where Key : notnull
{
    private int? _hashCode;

    /// <inheritdoc/>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Key Id { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    /// <inheritdoc/>
    public bool IsTransient()
    {
        if (typeof(Key) == typeof(long) || typeof(Key) == typeof(int) || typeof(Key) == typeof(Guid))
            return Id.Equals(default(Key));

        return false;
    }
    /// <inheritdoc />
    public object GetId() => Id;
    /// <inheritdoc />
    public override int GetHashCode()
    {
        if (!IsTransient())
        {
            if (!_hashCode.HasValue)
                _hashCode = Id.GetHashCode() ^ 31;

            return _hashCode.Value;    // XOR for random distribution. See: http://blogs.msdn.com/b/ericlippert/archive/2011/02/28/guidelines-and-rules-forgethashcode.aspx
        } else
            return base.GetHashCode();
    }
    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is null or not Entity<Key>)
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (GetType() != obj.GetType())
            return false;
        var item = (Entity<Key>)obj;
        return !item.IsTransient() && !IsTransient() && item.Id.Equals(Id);
    }
}
