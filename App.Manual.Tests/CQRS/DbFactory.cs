using App.Manual.Tests.CQRS.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SoftUnlimit.CQRS.Test;
using SoftUnlimit.Data.EntityFramework;
using System;
using System.Collections.Generic;
using System.Text;

namespace App.Manual.Tests.CQRS
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DbContextWrite>
    {
        public static readonly Type BaseEntityType = typeof(_EntityTypeBuilder<>);

        public const string ConnStringRead = "Persist Security Info=False;Initial Catalog=Example;Connection Timeout=120;Data Source=.; uid=sa; pwd=no";
        public const string ConnStringWrite = "Persist Security Info=False;Initial Catalog=Example;Connection Timeout=120;Data Source=.; uid=sa; pwd=no";


        public DbContextWrite CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<DbContextWrite>()
                .UseSqlServer(ConnStringWrite)
                .Options;

            var db = new DbContextWrite(options);
            db.OnModelCreatingCallback(m => db.OnModelCreating(BaseEntityType, m, _ => true));


            return db;
        }
    }
}
