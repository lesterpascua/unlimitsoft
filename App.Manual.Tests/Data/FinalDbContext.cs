using Microsoft.EntityFrameworkCore;
using SoftUnlimit.CQRS.Test.Configuration;
using SoftUnlimit.Data.EntityFramework;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.CQRS.Test.Data
{
    public class TestDbContext : EFCQRSDbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options)
            : base(options, null, null)
        {
        }

        protected override bool AcceptConfigurationType(Type type) => true;
        protected override Type EntityTypeBuilderBaseClass => typeof(_EntityTypeBuilder<>);
    }
}
