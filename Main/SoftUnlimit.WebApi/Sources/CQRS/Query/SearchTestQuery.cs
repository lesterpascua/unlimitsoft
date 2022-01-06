using Microsoft.EntityFrameworkCore;
using SoftUnlimit.CQRS.Query;
using SoftUnlimit.Data.EntityFramework;
using SoftUnlimit.Web.Model;
using SoftUnlimit.Web.Security;
using SoftUnlimit.WebApi.Model;
using SoftUnlimit.WebApi.Sources.Data;
using SoftUnlimit.WebApi.Sources.Data.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.WebApi.Sources.CQRS.Query
{
    public class SearchTestQuery : MyQuery<SearchModel<Customer>>, IQuerySearch
    {
        public SearchTestQuery(IdentityInfo user = null) :
            base(user)
        {
        }

        public SearchCustomer.FilterVM Filter { get; set; }

        public Paging Paging { get; set; }
        public IReadOnlyList<ColumnName> Order { get; set; }
    }

    /// <summary>
    /// Query Handler.
    /// </summary>
    public class SearchTestQueryHandler : IMyQueryHandler<SearchModel<Customer>, SearchTestQuery>
    {
        private readonly IMyQueryRepository<Customer> _customerQueryRepository;

        public SearchTestQueryHandler(IMyQueryRepository<Customer> customerQueryRepository)
        {
            _customerQueryRepository = customerQueryRepository;
        }

        public async Task<SearchModel<Customer>> HandleAsync(SearchTestQuery args, CancellationToken ct = default)
        {
            var query = _customerQueryRepository.FindAll();

            if (!string.IsNullOrEmpty(args.Filter?.Name))
                query = query.Where(p => p.Name.Contains(args.Filter.Name));

            var (total, result) = await query
                .ToSearchAsync(_customerQueryRepository, args.Paging, args.Order, ct);
            return new SearchModel<Customer>(total, result);
        }
    }
}
