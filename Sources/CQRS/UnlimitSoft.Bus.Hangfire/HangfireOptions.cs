#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using Hangfire.Logging;
using System;

namespace UnlimitSoft.Bus.Hangfire;


/// <summary>
/// 
/// </summary>
public class HangfireOptions
{
    /// <summary>
    /// Database scheme
    /// </summary>
    public string Scheme { get; set; }
    /// <summary>
    /// Database connection string.
    /// </summary>
    public string ConnectionString { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public TimeSpan SchedulePollingInterval { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int WorkerCount { get; set; }
    /// <summary>
    /// Hangfire logger level.
    /// </summary>
    public LogLevel Logger { get; set; }
}
