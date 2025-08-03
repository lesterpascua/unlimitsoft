using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace UnlimitSoft.CQRS.Command;


/// <summary>
/// 
/// </summary>
public interface ICommandBusUtility
{
    /// <summary>
    /// Remove all the recurring job existing in the command bus.
    /// </summary>
    /// <param name="inUse"></param>
    Task CleanUnusedRecurringJobAsync(string[] inUse);

    /// <summary>
    /// Deleted occurence of the job with the test condition
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="queueType"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<bool> DeleteAsync(Predicate<object> predicate, BusQueue queueType, CancellationToken ct = default);

    /// <summary>
    /// Check if exist a job with the test condition
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="queueType"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<bool> ExistAsync(Predicate<object> predicate, BusQueue queueType = BusQueue.Scheduled | BusQueue.Processing, CancellationToken ct = default);
    /// <summary>
    /// Search all the job with the test condition
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="queueType"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<List<object>> SearchAsync(Predicate<object> predicate, BusQueue queueType = BusQueue.Scheduled | BusQueue.Processing, CancellationToken ct = default);
}
