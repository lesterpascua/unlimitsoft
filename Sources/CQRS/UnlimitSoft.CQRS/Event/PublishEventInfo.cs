using System;

namespace UnlimitSoft.CQRS.Event;


/// <summary>
/// 
/// </summary>
public sealed class PublishEventInfo : IComparable<PublishEventInfo>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="created"></param>
    /// <param name="scheduled"></param>
    public PublishEventInfo(Guid id, DateTime created, DateTime? scheduled)
    {
        Id = id;
        Created = created;
        Scheduled = scheduled;
    }

    /// <summary>
    /// Identifier of the event
    /// </summary>
    public Guid Id { get; }
    /// <summary>
    /// Date where the event is scheduled
    /// </summary>
    public DateTime? Scheduled { get; init; }
    /// <summary>
    /// Date where the event is created
    /// </summary>
    public DateTime Created { get; init; }

    /// <inheritdoc />
    public int CompareTo(PublishEventInfo? other)
    {
        if (other is null)
            throw new ArgumentNullException(nameof(other), "Compare operand can't be null");

        var date1 = Scheduled ?? Created;
        var date2 = other.Scheduled ?? other.Created;

        var value = date1.CompareTo(date2);
        if (value == 0)
            return Id.CompareTo(other.Id);
        return value;
    }
    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is null || obj is not PublishEventInfo k)
            return false;
        return Id == k.Id;
    }
}