using SoftUnlimit.CQRS.Query;
using System;
using System.Collections.Generic;
using System.Text;

namespace App.Manual.Tests.CQRS.Query
{
    public class DummyQuery2 : Query<bool, QueryProps>
    {
        public DummyQuery2()
        {
        }
    }
}
