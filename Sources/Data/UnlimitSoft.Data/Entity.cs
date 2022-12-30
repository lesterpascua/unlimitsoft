using System;

namespace UnlimitSoft.Data;


/// <summary>
/// Base entity implementation.
/// </summary>
/// <typeparam name="TKey"></typeparam>f
public abstract class Entity<TKey> : IEntity where TKey : notnull
{
    private int? _hashCode;

    /// <inheritdoc/>
    public TKey Id { get; set; } = default!;

    /// <inheritdoc/>
    public bool IsTransient()
    {
        if (typeof(TKey) == typeof(long) || typeof(TKey) == typeof(int) || typeof(TKey) == typeof(Guid))
            return Id.Equals(default(TKey));

        return false;
    }
    /// <inheritdoc />
    public object GetId() => Id;
    /// <inheritdoc />
    public override int GetHashCode()
    {
        if (IsTransient())
            return base.GetHashCode();

        if (!_hashCode.HasValue)
            _hashCode = Id.GetHashCode() ^ 31;
        return _hashCode.Value;    // XOR for random distribution. See: http://blogs.msdn.com/b/ericlippert/archive/2011/02/28/guidelines-and-rules-forgethashcode.aspx
    }
    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is null or not Entity<TKey>)
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (GetType() != obj.GetType())
            return false;
        var item = (Entity<TKey>)obj;
        return !item.IsTransient() && !IsTransient() && item.Id.Equals(Id);
    }
}
