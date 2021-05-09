using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.CQRS.Query
{
    /// <summary>
    /// Indicate the command handler allow validate him self
    /// </summary>
    public interface IQueryHandlerValidator : IQueryHandler
    {
        /// <summary>
        /// Build fluen validation rules.
        /// </summary>
        /// <param name="validator"></param>
        IValidator BuildValidator(IValidator validator);
    }
}
