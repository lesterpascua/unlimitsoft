﻿using Microsoft.Extensions.DependencyInjection;
using UnlimitSoft.CQRS.DependencyInjection;
using UnlimitSoft.CQRS.Message;
using UnlimitSoft.CQRS.Query;
using UnlimitSoft.CQRS.Query.Validation;

namespace UnlimitSoft.Benchmark.UnlimitSoft.CQRS.Labs;


/// <summary>
/// Test UnlimitSoftDispatcher vs MediatR
/// </summary>
public class QueryDispatcherLab
{
    private readonly IServiceProvider _provider;
    private readonly IQueryDispatcher _dispatcher;

    public QueryDispatcherLab(bool validate)
    {
        var services = new ServiceCollection();

        services.AddQueryHandler(typeof(IQueryHandler<,>), validate, typeof(Program).Assembly);

        _provider = services.BuildServiceProvider();
        _dispatcher = _provider.GetRequiredService<IQueryDispatcher>();
    }

    public async Task<string?> Dispatch()
    {
        var query = new Query { Name = "Lester Pastrana" };
        var (_, body) = await query.ExecuteAsync(_dispatcher);

        return body;
    }

    #region Nested Classes
    /// <summary>
    /// 
    /// </summary>
    public class Query : Query<string, QueryProps>
    {
        /// <summary>
        /// 
        /// </summary>
        public string? Name { get; init; }
    }
    public class QueryHandler : IQueryHandler<string, Query>, IQueryHandlerValidator<Query>, IQueryHandlerCompliance<Query>
    {
        public Task<string> HandleAsync(Query query, CancellationToken ct = default)
        {
            var result = $"{query.Name} - {SysClock.GetUtcNow()}";
            return Task.FromResult(result);
        }

        public ValueTask<IQueryResponse> ComplianceAsync(Query query, CancellationToken ct = default)
        {
            return ValueTask.FromResult(query.QuickOkResponse());
        }

        public ValueTask<IQueryResponse> ValidatorAsync(Query query, QueryValidator<Query> validator, CancellationToken ct = default)
        {
            return ValueTask.FromResult(query.QuickOkResponse());
        }
    }
    #endregion
}