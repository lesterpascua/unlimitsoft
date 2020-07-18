using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.CQRS.Command.Validation
{
    /// <summary>
    /// Base class for validate all command if data of command is correct. 
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    public abstract class AbstractCommandValidator<TCommand> : AbstractValidator<CommandSharedCache<TCommand>>
        where TCommand : ICommand
    {
    }
}
