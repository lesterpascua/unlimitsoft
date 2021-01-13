using App.Manual.Tests.CQRS.Data;
using SoftUnlimit.Data;
using SoftUnlimit.Data.EntityFramework.Seed;
using System;
using System.Collections.Generic;
using System.Text;

namespace App.Manual.Tests.CQRS.Seed
{
    public class DummySeed : BaseCustomSeed<Dummy>
    {
        private readonly IDbUnitOfWork unitOfWork;

        public DummySeed(IDbUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
    }
}
