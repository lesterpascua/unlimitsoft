using Hangfire;
using Hangfire.States;
using Hangfire.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.Command;
using UnlimitSoft.Json;

namespace UnlimitSoft.Bus.Hangfire;


/// <summary>
/// 
/// </summary>
public sealed class HangfireCommandBusUtility : ICommandBusUtility
{
    private readonly IJsonSerializer _serializer;
    private readonly IBackgroundJobClient _jobClient;
    private readonly IRecurringJobManager _recurringJobManager;


    /// <summary>
    /// 
    /// </summary>
    /// <param name="serializer"></param>
    /// <param name="jobClient"></param>
    /// <param name="recurringJobManager"></param>
    public HangfireCommandBusUtility(IJsonSerializer serializer, IBackgroundJobClient jobClient, IRecurringJobManager recurringJobManager)
    {
        _serializer = serializer;
        _jobClient = jobClient;
        _recurringJobManager = recurringJobManager;
    }

    /// <inheritdoc />
    public Task CleanUnusedRecurringJobAsync(string[] inUse)
    {
        using var connection = JobStorage.Current.GetConnection();

        var data = connection.GetRecurringJobs();
        foreach (var entry in data)
        {
            if (Array.IndexOf(inUse, entry.Id) != -1)
                continue;
            _recurringJobManager.RemoveIfExists(entry.Id);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<bool> DeleteAsync(Predicate<object> predicate, BusQueue queueType, CancellationToken ct = default)
    {
        string? jobId = null;
        var found = Find(
            predicate,
            queueType,
            (id, _) =>
            {
                jobId = id;
                return false;
            }
        );
        if (!found || jobId is null)
            return Task.FromResult(false);

        var success = _jobClient.ChangeState(
            jobId, 
            new DeletedState { Reason = "HangfireCommandBusUtility:Manual" }
        );
        return Task.FromResult(success);
    }

    /// <inheritdoc />
    public Task<bool> ExistAsync(Predicate<object> predicate, BusQueue queueType, CancellationToken ct = default)
    {
        var success = Find(
            predicate, 
            queueType, 
            AllowNext
        );
        return Task.FromResult(success);

        // =======================================================================================================================================
        static bool AllowNext(string _1, object _2) => false;
    }
    /// <inheritdoc />
    public Task<List<object>> SearchAsync(Predicate<object> predicate, BusQueue queueType, CancellationToken ct = default)
    {
        var result = new List<object>();
        Find(
            predicate,
            queueType,
            (jobId, data) =>
            {
                result.Add(data);
                return true;
            }
        );
        return Task.FromResult(result);
    }

    #region Private Methods
    /// <summary>
    /// 
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="queueType"></param>
    /// <param name="allowNext">Indicate if allow the process to continue to check the next item or should stop immediately</param>
    /// <returns></returns>
    private bool Find(Predicate<object> predicate, BusQueue queueType, Func<string, object, bool> allowNext)
    {
        const int PAGE_SIZE = 50;

        var find = false;
        var api = JobStorage.Current.GetMonitoringApi();
        if (queueType.HasFlag(BusQueue.Enqueued))
        {
            var queues = api.Queues();
            foreach (var queue in queues)
            {
                var jobs = api.EnqueuedJobs(queue.Name, 0, int.MaxValue);
                foreach (var job in jobs.Where(x => x.Value.Job is not null))
                {
                    var type = (Type)job.Value.Job.Args[job.Value.Job.Args.Count - 1];
                    var data = _serializer.Deserialize(type, (string)job.Value.Job.Args[0])!;
                    if (!predicate(data))
                        continue;

                    find = true;
                    var moveNext = allowNext(job.Key, data);
                    if (!moveNext)
                        return true;
                }
            }
        }
        if (queueType.HasFlag(BusQueue.Scheduled))
        {
            var total = api.ScheduledCount();
            for (var i = 0; total > 0; i++)
            {
                var count = Math.Min(PAGE_SIZE, total);
                var jobs = api.ScheduledJobs(i * PAGE_SIZE, (int)count);
                foreach (var job in jobs.Where(x => x.Value.Job is not null))
                {
                    var type = (Type)job.Value.Job.Args[job.Value.Job.Args.Count - 1];
                    var data = _serializer.Deserialize(type, (string)job.Value.Job.Args[0])!;
                    if (!predicate(data))
                        continue;

                    find = true;
                    var moveNext = allowNext(job.Key, data);
                    if (!moveNext)
                        return true;
                }
                total -= PAGE_SIZE;
            }
        }
        if (queueType.HasFlag(BusQueue.Processing))
        {
            var total = api.ProcessingCount();
            for (var i = 0; total > 0; i++)
            {
                var count = Math.Min(PAGE_SIZE, total);
                var jobs = api.ProcessingJobs(i * PAGE_SIZE, (int)count);
                foreach (var job in jobs.Where(x => x.Value.Job is not null))
                {
                    var type = (Type)job.Value.Job.Args[job.Value.Job.Args.Count - 1];
                    var data = _serializer.Deserialize(type, (string)job.Value.Job.Args[0])!;
                    if (!predicate(data))
                        continue;

                    find = true;
                    var moveNext = allowNext(job.Key, data);
                    if (!moveNext)
                        return true;
                }
                total -= PAGE_SIZE;
            }
        }
        if (queueType.HasFlag(BusQueue.Succeeded))
        {
            var total = api.SucceededListCount();
            for (var i = 0; total > 0; i++)
            {
                var count = Math.Min(PAGE_SIZE, total);
                var jobs = api.SucceededJobs(i * PAGE_SIZE, (int)count);
                foreach (var job in jobs.Where(x => x.Value.Job is not null))
                {
                    var type = (Type)job.Value.Job.Args[job.Value.Job.Args.Count - 1];
                    var data = _serializer.Deserialize(type, (string)job.Value.Job.Args[0])!;
                    if (!predicate(data))
                        continue;

                    find = true;
                    var moveNext = allowNext(job.Key, data);
                    if (!moveNext)
                        return true;
                }
                total -= PAGE_SIZE;
            }
        }
        if (queueType.HasFlag(BusQueue.Deleted))
        {
            var total = api.DeletedListCount();
            for (var i = 0; total > 0; i++)
            {
                var count = Math.Min(PAGE_SIZE, total);
                var jobs = api.DeletedJobs(i * PAGE_SIZE, (int)count);
                foreach (var job in jobs.Where(x => x.Value.Job is not null))
                {
                    var type = (Type)job.Value.Job.Args[job.Value.Job.Args.Count - 1];
                    var data = _serializer.Deserialize(type, (string)job.Value.Job.Args[0])!;
                    if (!predicate(data))
                        continue;

                    find = true;
                    var moveNext = allowNext(job.Key, data);
                    if (!moveNext)
                        return true;
                }
                total -= PAGE_SIZE;
            }
        }
        if (queueType.HasFlag(BusQueue.Failed))
        {
            var total = api.FailedCount();
            for (var i = 0; total > 0; i++)
            {
                var count = Math.Min(PAGE_SIZE, total);
                var jobs = api.FailedJobs(i * PAGE_SIZE, (int)count);
                foreach (var job in jobs.Where(x => x.Value.Job is not null))
                {
                    var type = (Type)job.Value.Job.Args[job.Value.Job.Args.Count - 1];
                    var data = _serializer.Deserialize(type, (string)job.Value.Job.Args[0])!;
                    if (!predicate(data))
                        continue;

                    find = true;
                    var moveNext = allowNext(job.Key, data);
                    if (!moveNext)
                        return true;
                }
                total -= PAGE_SIZE;
            }
        }

        return find;
    }
    #endregion
}
