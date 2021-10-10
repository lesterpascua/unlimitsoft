using Hangfire;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.Bus.Hangfire
{
    /// <summary>
    /// 
    /// </summary>
    public interface IJobProcessor
    {
        /// <summary>
        /// Job identifier for this processor.
        /// </summary>
        BackgroundJob Metadata { get; internal set; }
        /// <summary>
        /// Cancelation token for this processor
        /// </summary>
        CancellationToken CancellationToken { get; internal set; }

        /// <summary>
        /// Process the logic asociate to some job.
        /// </summary>
        /// <param name="json">Command serialize as json</param>
        /// <param name="type">Type of the command</param>
        /// <returns></returns>
        Task ProcessAsync(string json, Type type);
    }
}
