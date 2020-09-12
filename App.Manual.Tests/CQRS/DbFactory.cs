using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SoftUnlimit.CQRS.Test;
using System;
using System.Collections.Generic;
using System.Text;

namespace App.Manual.Tests.CQRS
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DbContextWrite>
    {
        public const string ConnStringRead = "Persist Security Info=False;Initial Catalog=Example;Connection Timeout=120;Data Source=.; uid=sa; pwd=no";
        public const string ConnStringWrite = "Persist Security Info=False;Initial Catalog=Example;Connection Timeout=120;Data Source=.; uid=sa; pwd=no";


        public DbContextWrite CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<DbContextWrite>()
                .UseSqlServer(ConnStringWrite)
                .Options;

            return new DbContextWrite(options, null);
        }
    }
}
