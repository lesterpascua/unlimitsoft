using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.Data;
using UnlimitSoft.WebApi.Sources.Adapter;
using UnlimitSoft.WebApi.Sources.Data;
using UnlimitSoft.WebApi.Sources.Data.Model;
using UnlimitSoft.WebApi.Sources.Data.Repository;

namespace UnlimitSoft.WebApi.Controllers;


[ApiController]
[Route("[controller]")]
[AllowAnonymous]
public sealed class DatabaseController : ControllerBase
{
    private readonly IServiceScopeFactory _factory;
    private readonly IMyUnitOfWork _unitOfWork;
    private readonly ICustomerQueryRepository _customerQueryRepository;
    private readonly ILogger<DatabaseController> _logger;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="factory"></param>
    /// <param name="logger"></param>
    public DatabaseController(IServiceScopeFactory factory, IMyUnitOfWork unitOfWork, ICustomerQueryRepository customerQueryRepository, ILogger<DatabaseController> logger)
    {
        _factory = factory;
        _unitOfWork = unitOfWork;
        _customerQueryRepository = customerQueryRepository;
        _logger = logger;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<TestApiResponse>> Get(CancellationToken ct = default)
    {
        IDbConnectionFactory connFactory = _unitOfWork;
        using var aa1 = connFactory.CreateNewDbConnection();
        var aa2 = connFactory.TimeOut;

        var t1 = Task.Run(async () => await RunTask("t1", ct), ct);
        var t2 = Task.Run(async () => await RunTask("t2", ct), ct);

        await Task.WhenAll (t1, t2);
        return Ok(true);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpGet("compiled")]
    public async Task<ActionResult<Customer>> GetCompile(CancellationToken ct = default)
    {
        var customer = await _customerQueryRepository.GetByIdAsync(Guid.Parse("64846A4E-E5BA-4F40-B00A-B1483CDAED8E"), ct);
        return Ok(customer);
    }

    private async Task RunTask(string name, CancellationToken ct)
    {
        using var scope = _factory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IMyUnitOfWork>();
        var lockRepository = scope.ServiceProvider.GetRequiredService<IMyRepository<Lock>>();
        var customerRepository = scope.ServiceProvider.GetRequiredService<IMyRepository<Customer>>();


        // TransactionCreateAsync
        using var transaction = await unitOfWork.TransactionCreateAsync(IsolationLevel.Unspecified, ct);

        _logger.LogInformation("{Name}: TransactionCreateAsync", name);

        // Fetch customer
        var customer = await customerRepository.FindAll().ToArrayAsync(ct);
        _logger.LogInformation("{Name}: Fetch customer", name);

        // Update Lock
        var lockObject = await lockRepository.FindAll().FirstOrDefaultAsync(cancellationToken: ct);
        if (lockObject == null)
        {
            lockObject = new Lock { Id = 1, DateTime = SysClock.GetUtcNow() };
            await lockRepository.AddAsync(lockObject, ct);
        }
        else
        {
            lockObject.DateTime = name == "t1" ? SysClock.GetUtcNow() : SysClock.GetUtcNow().AddDays(1);
            lockRepository.Update(lockObject);
        }
        await unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("{Name}: Update Lock", name);

        // TransactionRollbackAsync
        await Task.Delay(TimeSpan.FromSeconds(5), ct);
        await unitOfWork.TransactionCommitAsync(ct);
        _logger.LogInformation("{Name}: TransactionCommitAsync", name);
    }
}
