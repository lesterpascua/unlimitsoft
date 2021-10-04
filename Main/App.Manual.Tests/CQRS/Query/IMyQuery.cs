using SoftUnlimit.CQRS.Query;
using System;
using System.Collections.Generic;
using System.Text;

namespace App.Manual.Tests.CQRS.Query
{
    public interface IMyQueryHandler : IQueryHandler
    {
    }
    public interface IMyQueryHandler<TResult, TQuery> : IQueryHandler<TResult, TQuery>, IMyQueryHandler
        where TQuery : IQuery<TResult>
    {
    }
}
