﻿using UnlimitSoft.CQRS.Query;

namespace UnlimitSoft.WebApi.Sources.CQRS.Query
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="TQuery"></typeparam>
    public interface IMyQueryHandler<TResult, TQuery> : IQueryHandler<TResult, TQuery>
        where TQuery : MyQuery<TResult>
    {
    }
}
