using SoftUnlimit.Data;
using SoftUnlimit.Data.EntityFramework.Seed;
using SoftUnlimit.WebApi.Sources.Data.Model;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.WebApi.Sources.Data.Seed
{
    public class CustomerSeed : BaseCustomSeed<Customer>
    {
        private readonly IMyRepository<Customer> _customerRepository;

        public CustomerSeed(IUnitOfWork unitOfWork, IMyRepository<Customer> customerRepository, int priority = 1000) :
            base(unitOfWork, priority)
        {
            _customerRepository = customerRepository;
        }

        public override async Task SeedAsync(CancellationToken ct = default)
        {
            await _customerRepository.AddRangeAsync(new Customer[] {
                new Customer { Id = Guid.NewGuid(), Name = "Lester" },
                new Customer { Id = Guid.NewGuid(), Name = "Dianelis" },
                new Customer { Id = Guid.NewGuid(), Name = "Liam" }
            }, ct);
        }
    }
}
