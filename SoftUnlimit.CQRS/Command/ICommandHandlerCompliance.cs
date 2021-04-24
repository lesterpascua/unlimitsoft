using FluentValidation;
using SoftUnlimit.CQRS.Message;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Command
{
    /// <summary>
    /// Indicate the command handler allow compliance him self
    /// </summary>
    public interface ICommandHandlerCompliance : ICommandHandler
    {
        /// <summary>
        /// Evaluate compliance.
        /// </summary>
        /// <param name="command"></param>
        Task<CommandResponse> HandleComplianceAsync(ICommand command);
    }
}
