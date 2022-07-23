using UnlimitSoft.CQRS.Command;
using System;

namespace UnlimitSoft.CQRS.Command
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
