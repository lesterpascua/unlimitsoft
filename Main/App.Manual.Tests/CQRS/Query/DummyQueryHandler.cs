using SoftUnlimit.CQRS.Query;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace App.Manual.Tests.CQRS.Query
{
    public class DummyQueryHandler
        : IMyQueryHandler<bool, DummyQuery>
    {
        public Task<bool> HandlerAsync(DummyQuery args)
        {
            return Task.FromResult(true);
        }

        public Task<bool> HandlerAsync(DummyQuery query, CancellationToken ct = default)
        {
            return Task.FromResult(true);
        }
    }
}
