using Hangfire;
using System;
using System.Threading.Tasks;
using UnlimitSoft.Message;

namespace UnlimitSoft.Bus.Hangfire;


/// <summary>
/// 
/// </summary>
public interface IJobProcessor
{
    /// <summary>
    /// Context of command execution
    /// </summary>
    JobActivatorContext Context { get; set; }

    /// <summary>
    /// Process the logic asociate to some job.
    /// </summary>
    /// <param name="json">Command serialize as json</param>
    /// <param name="type">Type of the command</param>
    /// <returns></returns>
    Task<IResponse> ProcessAsync(string json, Type type);
}
