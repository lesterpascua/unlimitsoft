using Microsoft.Extensions.DependencyInjection;
using UnlimitSoft.CQRS.DependencyInjection;
using UnlimitSoft.CQRS.Query;
using UnlimitSoft.Mediator;
using UnlimitSoft.Mediator.Validation;
using UnlimitSoft.Message;

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
        var response = await _dispatcher.DispatchAsync(_provider, query);

        return response.Value;
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
    public class QueryHandler : IQueryHandler<Query, string>, IQueryHandlerValidator<Query>, IQueryHandlerCompliance<Query>
    {
        public ValueTask<string> HandleV2Async(Query query, CancellationToken ct = default)
        {
            var result = $"{query.Name} - {SysClock.GetUtcNow()}";
            return ValueTask.FromResult(result);
        }

        public ValueTask<IResponse> ComplianceV2Async(Query query, CancellationToken ct = default)
        {
            return ValueTask.FromResult(query.OkResponse());
        }

        public ValueTask<IResponse> ValidatorV2Async(Query query, RequestValidator<Query> validator, CancellationToken ct = default)
        {
            return ValueTask.FromResult(query.OkResponse());
        }
    }
    #endregion
}