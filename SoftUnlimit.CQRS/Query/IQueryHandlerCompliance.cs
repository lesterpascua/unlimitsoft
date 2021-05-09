using FluentValidation;
using SoftUnlimit.CQRS.Message;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Query
{
    /// <summary>
    /// Indicate the command handler allow compliance him self
    /// </summary>
    public interface IQueryHandlerCompliance : IQueryHandler
    {
        /// <summary>
        /// Evaluate compliance.
        /// </summary>
        /// <param name="query"></param>
        Task<QueryResponse> HandleComplianceAsync(IQuery query);
    }
}
