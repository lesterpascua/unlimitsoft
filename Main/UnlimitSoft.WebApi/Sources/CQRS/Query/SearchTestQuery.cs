﻿using Microsoft.EntityFrameworkCore;
using UnlimitSoft.CQRS.Query;
using UnlimitSoft.Data.EntityFramework;
using UnlimitSoft.Web.Model;
using UnlimitSoft.Web.Security;
using UnlimitSoft.WebApi.Model;
using UnlimitSoft.WebApi.Sources.Data;
using UnlimitSoft.WebApi.Sources.Data.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UnlimitSoft.WebApi.Sources.CQRS.Query
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
