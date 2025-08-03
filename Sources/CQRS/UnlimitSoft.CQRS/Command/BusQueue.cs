using System;

namespace UnlimitSoft.CQRS.Command;


/// <summary>
/// 
/// </summary>
[Flags]
public enum BusQueue
{
    /// <summary>
    /// 
    /// </summary>
    Scheduled = 1,
    /// <summary>
    /// 
    /// </summary>
    Processing = 2,
    /// <summary>
    /// 
    /// </summary>
    Deleted = 4,
    /// <summary>
    /// 
    /// </summary>
    Succeeded = 8,
    /// <summary>
    /// 
    /// </summary>
    Failed = 16,
    /// <summary>
    /// Jobs enqueued
    /// </summary>
    Enqueued = 32,
}