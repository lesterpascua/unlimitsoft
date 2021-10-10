using SoftUnlimit.CQRS.Command;
using System;

namespace SoftUnlimit.Bus.Hangfire
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISchedulerCommand : ICommand
    {
        /// <summary>
        /// Time to delay this command before procesed
        /// </summary>
        TimeSpan? Delay { get; set; }
    }
}
