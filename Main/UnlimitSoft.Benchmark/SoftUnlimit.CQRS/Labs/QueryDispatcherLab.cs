using Microsoft.Extensions.DependencyInjection;
using SoftUnlimit.CQRS.DependencyInjection;
using SoftUnlimit.CQRS.Message;
using SoftUnlimit.CQRS.Query;
using SoftUnlimit.CQRS.Query.Validation;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace UnlimitSoft.Benchmark.SoftUnlimit.CQRS.Labs
{
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

            services.AddQueryHandler(typeof(IQueryHandler<,>), null, validate, typeof(Program).Assembly);

            _provider = services.BuildServiceProvider();
            _dispatcher = _provider.GetRequiredService<IQueryDispatcher>();
        }

        public async Task<string> Dispatch()
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
        public class CommandHandler : IQueryHandler<string, Query>, IQueryHandlerValidator<Query>, IQueryHandlerCompliance<Query>
        {
            public Task<string> HandleAsync(Query query, CancellationToken ct = default)
            {
                var result = $"{query.Name} - {DateTime.UtcNow}";
                return Task.FromResult(result);
            }

            public ValueTask<IQueryResponse> ComplianceAsync(Query query, CancellationToken ct = default)
            {
                var a = query!.OkResponse();
                return ValueTask.FromResult(a);
            }

            public ValueTask ValidatorAsync(Query query, QueryValidator<Query> validator, CancellationToken ct = default)
            {
                return ValueTask.CompletedTask;
            }
        }
        #endregion
    }
}