using App.Manual.Tests.CQRS.Command;
using App.Manual.Tests.CQRS.Command.Events;
using App.Manual.Tests.CQRS.Data;
using App.Manual.Tests.CQRS.Query;
using Microsoft.EntityFrameworkCore;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Event.Json;
using SoftUnlimit.CQRS.EventSourcing.Json;
using SoftUnlimit.CQRS.Query;
using SoftUnlimit.Data;
using SoftUnlimit.Data.EntityFramework;
using SoftUnlimit.Data.Seed;
using SoftUnlimit.Map;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Manual.Tests.CQRS
{
    public class Startup
    {
        private readonly IMapper _mapper;
        private readonly IQueryDispatcher _queryDispatcher;
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEventNameResolver _eventCommandResolver;
        private readonly IRepository<JsonVersionedEventPayload> _versionedEventRepository;

        public Startup(
            IMapper mapper,
            IQueryDispatcher queryDispatcher, 
            ICommandDispatcher commandDispatcher, 
            IUnitOfWork unitOfWork, 
            IEventNameResolver eventCommandResolver,
            IRepository<JsonVersionedEventPayload> versionedEventRepository)
        {
            _mapper = mapper;
            _queryDispatcher = queryDispatcher;
            _commandDispatcher = commandDispatcher;
            _unitOfWork = unitOfWork;
            _eventCommandResolver = eventCommandResolver;
            _versionedEventRepository = versionedEventRepository;
        }

        /// <summary>
        /// Start example flow.
        /// </summary>
        /// <returns></returns>
        public async Task Start()
        {
            var sourceId = "0038f36f-6d6c-eab2-0000-000100000000";
            var rpo = new JsonEventSourcedRepository<Dummy>(_versionedEventRepository, _eventCommandResolver);
            var entity = await rpo.FindById(sourceId);

            var eventPayload = await _versionedEventRepository
                .Find(p => p.SourceId == sourceId)
                .OrderByDescending(k => k.Version)
                .FirstOrDefaultAsync();
            var tr = eventPayload.Transform<DummyDTO>(_mapper, _eventCommandResolver);


            var (queryResponse, result) = await new DummyQuery().ExecuteAsync(_queryDispatcher);
            if (queryResponse.IsSuccess)
                Console.WriteLine(queryResponse);

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
