using System;
using UnlimitSoft.CQRS.Command;
using UnlimitSoft.Web.Security;

namespace UnlimitSoft.WebApi.Sources.CQRS.Command;


public interface IMyCommand
{
    MyCommandProps Props { get; }
}
public class MyCommand<TResponse> : Command<TResponse, MyCommandProps>, IMyCommand
{
    public MyCommand() { }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="user"></param>
    public MyCommand(Guid id, IdentityInfo? user)
    {
        Props = new MyCommandProps
        {
            Id = id,
            Name = GetType().Name,
            User = user
        };
    }
}
public class MySchedulerCommand<TResponse> : MyCommand<TResponse>, ISchedulerCommand
{
    public MySchedulerCommand() { }
    public MySchedulerCommand(Guid id, IdentityInfo? user = null) :
        base(id, user) 
    { 
    }

    public object? GetJobId() => Props!.JobId;
    public void SetJobId(object? jobId) => Props!.JobId = (string?)jobId;

    public int GetRetry() => Props!.Retry;
    public void SetRetry(int retry) => Props!.Retry = retry;

    public TimeSpan? GetDelay() => Props!.Delay;
    public void SetDelay(TimeSpan? delay) => Props!.Delay = delay;
}
