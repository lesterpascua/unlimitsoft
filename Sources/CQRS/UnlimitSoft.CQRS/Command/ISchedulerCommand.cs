using System;

namespace UnlimitSoft.CQRS.Command;


/// <summary>
/// Props asociate to a command of with scheduler time
/// </summary>
public interface ISchedulerCommand
{
    /// <summary>
    /// Get jobId asociate with the command.
    /// </summary>
    public object GetJobId();
    /// <summary>
    /// Set the jobId asociate with the command.
    /// </summary>
    /// <param name="jobId"></param>
    public void SetJobId(object jobId);

    /// <summary>
    /// Get indicate how many time this command is retry
    /// </summary>
    public int? GetRetry();
    /// <summary>
    /// Set indicate how many time this command is retry
    /// </summary>
    /// <param name="retry"></param>
    public void SetRetry(int? retry);

    /// <summary>
    /// Get time to delay this command before procesed
    /// </summary>
    public TimeSpan? GetDelay();
    /// <summary>
    /// Set time to delay this command before procesed
    /// </summary>
    public void SetDelay(TimeSpan? dalay);
}