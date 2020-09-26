using SoftUnlimit.CQRS.Query;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace App.Manual.Tests.CQRS.Query
{
    public class DummyQueryHandler2
        : IQueryHandler<bool, DummyQuery>
    {
        public Task<bool> HandlerAsync(DummyQuery args)
        {
            return Task.FromResult(true);
        }
    }
}
