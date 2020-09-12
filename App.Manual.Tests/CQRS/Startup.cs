using App.Manual.Tests.CQRS.Command;
using Microsoft.EntityFrameworkCore;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.Data;
using SoftUnlimit.Data.EntityFramework;
using SoftUnlimit.Data.Seed;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;

namespace App.Manual.Tests.CQRS
{
    public class Startup
    {
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IUnitOfWork _unitOfWork;

        public Startup(ICommandDispatcher commandDispatcher, IUnitOfWork unitOfWork)
        {
            _commandDispatcher = commandDispatcher;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Start example flow.
        /// </summary>
        /// <returns></returns>
        public async Task Start()
        {
            await SeedHelper.Seed(_unitOfWork, typeof(Startup).Assembly, (unitOfWork) => {
                if (unitOfWork is DbContext dbContext)
                    return dbContext.Database.MigrateAsync();
                return Task.CompletedTask;
            });

            var command = new DummyCreateCommand {
                Name = "Hello CQRS"
            };

            var response = await _commandDispatcher.DispatchAsync(command);
            Console.WriteLine(response);
        }
    }
}
