using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftUnlimit.Data;
using SoftUnlimit.WebApi.Sources.Adapter;
using SoftUnlimit.WebApi.Sources.Data;
using SoftUnlimit.WebApi.Sources.Data.Model;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [AllowAnonymous]
    public class DatabaseController : ControllerBase
    {
        private readonly IServiceScopeFactory _factory;
        private readonly IMyUnitOfWork _unitOfWork;
        private readonly ILogger<DatabaseController> _logger;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="logger"></param>
        public DatabaseController(IServiceScopeFactory factory, IMyUnitOfWork unitOfWork, ILogger<DatabaseController> logger)
        {
            _factory = factory;
            _unitOfWork = unitOfWork;
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

        private async Task RunTask(string name, CancellationToken ct)
        {
            using var scope = _factory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IMyUnitOfWork>();
            var lockRepository = scope.ServiceProvider.GetRequiredService<IMyRepository<Lock>>();
            var customerRepository = scope.ServiceProvider.GetRequiredService<IMyRepository<Customer>>();


            // TransactionCreateAsync
            await using var transaction = await unitOfWork.TransactionCreateAsync(IsolationLevel.Unspecified, ct);

            _logger.LogInformation($"{name}: TransactionCreateAsync");

            // Fetch customer
            var customer = await customerRepository.FindAll().ToArrayAsync(ct);
            _logger.LogInformation($"{name}: Fetch customer");

            // Update Lock
            var lockObject = await lockRepository.FindAll().FirstOrDefaultAsync(cancellationToken: ct);
            if (lockObject == null)
            {
                lockObject = new Lock { Id = 1, DateTime = DateTime.UtcNow };
                await lockRepository.AddAsync(lockObject, ct);
            }
            else
            {
                lockObject.DateTime = name == "t1" ? DateTime.UtcNow : DateTime.UtcNow.AddDays(1);
                lockRepository.Update(lockObject);
            }
            await unitOfWork.SaveChangesAsync(ct);
            _logger.LogInformation($"{name}: Update Lock");

            // TransactionRollbackAsync
            await Task.Delay(TimeSpan.FromSeconds(5), ct);
            await unitOfWork.TransactionCommitAsync(ct);
            _logger.LogInformation($"{name}: TransactionCommitAsync");
        }
    }
}
