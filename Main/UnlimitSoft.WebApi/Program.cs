using Medallion.Threading;
using Medallion.Threading.SqlServer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.WebApi
{
    public interface IOneJNLock
    {
        /// <summary>
        /// Distributed lock acros OneJN system
        /// </summary>
        /// <param name="name"></param>
        /// <param name="timeout">If time out is null wait infinite time</param>
        /// <param name="ct"></param>
        /// <exception cref="TimeoutException">If the lock is grather of the time out.</exception>
        /// <returns></returns>
        ValueTask<IDistributedSynchronizationHandle> AcquireAsync(string name, TimeSpan? timeout = null, CancellationToken ct = default);

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        /// <summary>
        /// Distributed lock acros OneJN system
        /// </summary>
        /// <param name="name"></param>
        /// <param name="timeout">By default don't wait, if is lock return null.</param>
        /// <param name="ct"></param>
        /// <returns>If is lock return null, else return the lock handler.</returns>
        ValueTask<IDistributedSynchronizationHandle?> TryAcquireAsync(string name, TimeSpan timeout = default, CancellationToken ct = default);
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
    }
    /// <summary>
    /// Using sql for lock operations.
    /// </summary>
    public class SqlServerOneJNLock : IOneJNLock
    {
        private readonly string _connString;
        private readonly ConcurrentDictionary<string, IDistributedLock> _distributedLockBucket;

        public SqlServerOneJNLock(string connString)
        {
            _connString = connString;
            _distributedLockBucket = new ConcurrentDictionary<string, IDistributedLock>();
        }

        /// <inheritdoc />
        public async ValueTask<IDistributedSynchronizationHandle> AcquireAsync(string name, TimeSpan? timeout = null, CancellationToken ct = default)
        {
            var distributedLock = AddIfNotExist(name);
            return await distributedLock.AcquireAsync(timeout, ct);
        }
        /// <inheritdoc />
        public async ValueTask<IDistributedSynchronizationHandle> TryAcquireAsync(string name, TimeSpan timeout = default, CancellationToken ct = default)
        {
            var distributedLock = AddIfNotExist(name);
            return await distributedLock.TryAcquireAsync(timeout, ct);
        }

        private IDistributedLock AddIfNotExist(string name)
        {
            if (!_distributedLockBucket.TryGetValue(name, out var distributedLock))
            {
                var tmp = new SqlDistributedLock(name, _connString);
                distributedLock = _distributedLockBucket.TryAdd(name, tmp) ? tmp : _distributedLockBucket[name];
            }

            return distributedLock;
        }
    }



    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
                .UseSerilog();
    }
}
