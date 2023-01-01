using System;

namespace UnlimitSoft.EventBus;

/// <summary>
/// Allow set the real name of the queue or topic in the enum attribute
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class QueueOrTopicNameAttribute : Attribute
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    public QueueOrTopicNameAttribute(string name)
    {
        Name = name;
    }

    /// <summary>
    /// 
    /// </summary>
    public string Name { get; }
}