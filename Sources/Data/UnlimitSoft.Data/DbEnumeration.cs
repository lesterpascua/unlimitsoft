using System;

namespace UnlimitSoft.Data;


/// <summary>
/// 
/// </summary>
public abstract class DbEnumeration<TKey> : Entity<TKey>, IDbEnumeration
    where TKey : Enum
{
    /// <summary>
    /// 
    /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    protected DbEnumeration()
    {
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    protected DbEnumeration(TKey id, string name)
    {
        Id = id;
        Name = name;
    }


    /// <summary>
    /// 
    /// </summary>
    public string Name { get; protected set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public int CompareTo(object? other) => Id.CompareTo(((DbEnumeration<TKey>)other!).Id);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode() => Id.GetHashCode();
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override string ToString() => Name;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object? obj)
    {
        if (obj is not DbEnumeration<TKey> otherValue)
            return false;

        bool valueMatches = Id.Equals(otherValue.Id);
        bool typeMatches = GetType().Equals(obj.GetType());

        return typeMatches && valueMatches;
    }
}