using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.WebApi.Sources.Data.Model;

namespace UnlimitSoft.WebApi.Sources.Data.Repository;


public interface ICustomerQueryRepository
{
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default);
}
public sealed class CustomerQueryRepository : ICustomerQueryRepository
{
    private readonly IMyQueryRepository<Customer> _queryRepository;

    private static Func<DbContextRead, Guid, Task<Customer?>> _getById = default!;


    public CustomerQueryRepository(IMyQueryRepository<Customer> queryRepository)
    {
        _queryRepository = queryRepository;
    }

    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        if (_getById is null)
            Interlocked.CompareExchange(
                ref _getById,
                EF.CompileAsyncQuery((DbContextRead context, Guid id) => context.Set<Customer>().FirstOrDefault(x => x.Id == id)),
                null
            );
        return _getById(_queryRepository.DbContext, id);
    }
}
