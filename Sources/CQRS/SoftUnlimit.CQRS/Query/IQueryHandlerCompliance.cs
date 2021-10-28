﻿using FluentValidation;
using SoftUnlimit.CQRS.Message;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Query
{
    /// <summary>
    /// Indicate the command handler allow compliance him self
    /// </summary>
    public interface IQueryHandlerCompliance<TQuery> : IQueryHandler
        where TQuery: IQuery
    {
        /// <summary>
        /// Evaluate compliance.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="ct"></param>
        Task<IQueryResponse> HandleComplianceAsync(TQuery query, CancellationToken ct = default);
    }
}