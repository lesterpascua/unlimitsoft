using SoftUnlimit.CQRS.Message;
using SoftUnlimit.CQRS.Query;
using SoftUnlimit.CQRS.Query.Compliance;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace App.Manual.Tests.CQRS.Query.Validator
{
    public class IDummyQueryCompliande : IQueryCompliance<DummyQuery>
    {
        public Task<QueryResponse> ExecuteAsync(DummyQuery args)
        {
            return Task.FromResult(args.OkResponse(true));
        }

        public Task<QueryResponse> ExecuteAsync(IQuery args)
        {
            return ExecuteAsync((DummyQuery)args);
        }
    }
}
