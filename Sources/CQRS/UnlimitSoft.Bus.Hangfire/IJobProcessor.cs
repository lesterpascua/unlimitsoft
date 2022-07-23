using Hangfire;
using UnlimitSoft.CQRS.Message;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace UnlimitSoft.Bus.Hangfire;


/// <summary>
/// 
/// </summary>
public interface IJobProcessor
{
    /// <summary>
    /// Job identifier for this processor.
    /// </summary>
    BackgroundJob Metadata { get; set; }
    /// <summary>
    /// Cancelation token for this processor
    /// </summary>
    CancellationToken CancellationToken { get; set; }

    /// <summary>
    /// Process the logic asociate to some job.
    /// </summary>
    /// <param name="json">Command serialize as json</param>
    /// <param name="type">Type of the command</param>
    /// <returns></returns>
    Task<ICommandResponse> ProcessAsync(string json, Type type);
}
