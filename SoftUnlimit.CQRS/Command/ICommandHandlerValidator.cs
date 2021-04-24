using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.CQRS.Command
{
    /// <summary>
    /// Indicate the command handler allow validate him self
    /// </summary>
    public interface ICommandHandlerValidator : ICommandHandler
    {
        /// <summary>
        /// Build fluen validation rules.
        /// </summary>
        /// <param name="validator"></param>
        IValidator BuildValidator(IValidator validator);
    }
}
