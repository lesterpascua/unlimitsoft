using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.Data.EntityFramework.Seed;
using UnlimitSoft.WebApi.Sources.Data.Model;

namespace UnlimitSoft.WebApi.Sources.Data.Seed;


public sealed class CustomerSeed : BaseCustomSeed<Customer>
{
    private readonly IMyRepository<Customer> _customerRepository;

    public CustomerSeed(IMyUnitOfWork unitOfWork, IMyRepository<Customer> customerRepository, int priority = 1000) 
        : base(unitOfWork, priority)
    {
        _customerRepository = customerRepository;
    }

    public override async Task SeedAsync(CancellationToken ct = default)
    {
        var seed = new List<Customer>();
        if (await _customerRepository.FindAll().AnyAsync(p => p.Name == "Lester", ct) == false)
            seed.Add(new Customer { Id = Guid.NewGuid(), Name = "Lester" });
        if (await _customerRepository.FindAll().AnyAsync(p => p.Name == "Dianelis", ct) == false)
            seed.Add(new Customer { Id = Guid.NewGuid(), Name = "Dianelis" });
        if (await _customerRepository.FindAll().AnyAsync(p => p.Name == "Liam", ct) == false)
            seed.Add(new Customer { Id = Guid.NewGuid(), Name = "Liam" });

        if (seed.Count != 0)
            await _customerRepository.AddRangeAsync(seed, ct);
    }
}
