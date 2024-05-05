using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace UnlimitSoft.Threading;


/// <summary>
/// Create a Task pool for operation to be runned in the background. Those operation will run in memory so if there is a fail the operation will never recovery
/// </summary>
/// <remarks>
/// Use this to run operation in background without block the main thread. This is ideal to run find and forget operations.
/// </remarks>
public sealed class TasksPool
{
    private static TasksPool? _instance;
    private readonly ConcurrentDictionary<int, TaskPoolInfo> _tasks;

    private TasksPool()
    {
        _tasks = new ConcurrentDictionary<int, TaskPoolInfo>();
    }

    /// <summary>
    /// Get instance of the Task Pool
    /// </summary>
    public static TasksPool Instance
    {
        get
        {
            if (_instance is null)
                Interlocked.CompareExchange(ref _instance, new TasksPool(), null);
            return _instance;
        }
    }
    /// <summary>
    /// Get the current executing task
    /// </summary>
    public TaskPoolInfo[] Current => [.. _tasks.Values];

    /// <summary>
    /// Run an operation in a background task
    /// </summary>
    /// <param name="name"></param>
    /// <param name="action"></param>
    /// <param name="cleanMemory"></param>
    /// <returns></returns>
    public int Run(string name, Action action, bool cleanMemory)
    {
        var id = 0;
        var task = Task.Run(async () =>
        {
            try
            {
                action();
                if (cleanMemory)
                    GC.Collect(1, GCCollectionMode.Forced, true);
            }
            finally
            {
                for (var i = 0; id == 0 && i < 100; i++)
                    await Task.Delay(100);
                if (id != 0)
                    _tasks.TryRemove(id, out _);
            }
        });
        _tasks.TryAdd(task.Id, new TaskPoolInfo(name, task));
        id = task.Id;

        return id;
    }
    /// <summary>
    ///  Run an async operation in a background task
    /// </summary>
    /// <param name="name"></param>
    /// <param name="action"></param>
    /// <param name="cleanMemory"></param>
    /// <returns></returns>
    public int Run(string name, Func<Task> action, bool cleanMemory)
    {
        var id = 0;
        var task = Task.Run(async () =>
        {
            try
            {
                await action();
                if (cleanMemory)
                    GC.Collect(1, GCCollectionMode.Forced, true);
            }
            finally
            {
                for (var i = 0; id == 0 && i < 100; i++)
                    await Task.Delay(100);
                if (id != 0)
                    _tasks.TryRemove(id, out _);
            }
        });
        _tasks.TryAdd(task.Id, new TaskPoolInfo(name, task));
        id = task.Id;

        return id;
    }
}
