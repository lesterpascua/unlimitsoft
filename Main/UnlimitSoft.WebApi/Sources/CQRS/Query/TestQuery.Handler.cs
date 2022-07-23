using Microsoft.EntityFrameworkCore;
using UnlimitSoft.WebApi.Sources.Data;
using UnlimitSoft.WebApi.Sources.Data.Model;
using System.Threading;
using System.Threading.Tasks;

namespace UnlimitSoft.WebApi.Sources.CQRS.Query
{
    /// <summary>
    /// Handler definitial (you can choose put all in the same file or separate using partial class)
    /// </summary>
    public partial class TestQueryHandler : IMyQueryHandler<Customer[], TestQuery>
    {
        private readonly IMyQueryRepository<Customer> _customerQueryRepository;

        public TestQueryHandler(IMyQueryRepository<Customer> customerQueryRepository)
        {
            _customerQueryRepository = customerQueryRepository;
        }

        public async Task<Customer[]> HandleAsync(TestQuery query, CancellationToken ct = default)
        {
            var data = await _customerQueryRepository.FindAll().ToArrayAsync();
            return data;
        }
    }
}
