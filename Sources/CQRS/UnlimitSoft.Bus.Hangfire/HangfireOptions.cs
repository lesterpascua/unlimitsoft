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
    public string Scheme { get; set; } = default!;
    /// <summary>
    /// Database connection string.
    /// </summary>
    public string ConnectionString { get; set; } = default!;
    /// <summary>
    /// 
    /// </summary>
    public TimeSpan SchedulePollingInterval { get; set; } = TimeSpan.FromSeconds(30);
    /// <summary>
    /// 
    /// </summary>
    public int WorkerCount { get; set; } = 1;
    /// <summary>
    /// Hangfire logger level.
    /// </summary>
    public LogLevel Logger { get; set; }
    /// <summary>
    /// Create hangfire publisher but not up the server
    /// </summary>
    public bool WithoutServer { get; set; }
}
