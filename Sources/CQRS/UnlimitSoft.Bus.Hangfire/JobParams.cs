using System;

namespace UnlimitSoft.Bus.Hangfire;


/// <summary>
/// 
/// </summary>
internal sealed class JobParams
{
    public int Retry { get; set; }
    public TimeSpan? Delay { get; set; }
}
