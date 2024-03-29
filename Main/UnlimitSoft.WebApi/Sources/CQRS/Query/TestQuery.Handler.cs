﻿using Microsoft.EntityFrameworkCore;
using UnlimitSoft.WebApi.Sources.Data;
using UnlimitSoft.WebApi.Sources.Data.Model;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.Web.Model;
using UnlimitSoft.CQRS.Query;

namespace UnlimitSoft.WebApi.Sources.CQRS.Query;


/// <summary>
/// Handler definitial (you can choose put all in the same file or separate using partial class)
/// </summary>
public partial class TestQueryHandler : IMyQueryHandler<TestQuery, SearchModel<Customer>>, IQueryHandlerLifeCycle<TestQuery>
{
    private readonly IMyQueryRepository<Customer> _customerQueryRepository;

    public TestQueryHandler(IMyQueryRepository<Customer> customerQueryRepository)
    {
        _customerQueryRepository = customerQueryRepository;
    }

    public ValueTask InitAsync(TestQuery request, CancellationToken ct = default)
    {
        return ValueTask.CompletedTask;
    }

    public async ValueTask<SearchModel<Customer>> HandleAsync(TestQuery query, CancellationToken ct = default)
    {
        var data = await _customerQueryRepository.FindAll().ToArrayAsync(ct);
        var model = new SearchModel<Customer>(data.Length, data);
        return model;
    }

    public ValueTask EndAsync(TestQuery request, CancellationToken ct = default)
    {
        return ValueTask.CompletedTask;
    }
}
