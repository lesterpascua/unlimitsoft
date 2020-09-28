using App.Manual.Tests.CQRS.Data;
using AutoMapper;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Message;
using SoftUnlimit.Data;
using SoftUnlimit.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace App.Manual.Tests.CQRS.Command
{
    public class DummyCommandHandler :
        ICommandHandler<DummyCreateCommand>
    {
        private readonly IMapper _mapper;
        private readonly IIdGenerator<Guid> _gen;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Dummy> _dummyRepository;

        public DummyCommandHandler(
            IMapper mapper,
            IIdGenerator<Guid> gen,
            IUnitOfWork unitOfWork,
            IRepository<Dummy> dummyRepository
        )
        {
            _mapper = mapper;
            _gen = gen;
            _unitOfWork = unitOfWork;
            _dummyRepository = dummyRepository;
        }

        public async Task<CommandResponse> HandleAsync(DummyCreateCommand command, object validationCache)
        {
            var dbObj = new Dummy {
                Id = _gen.GenerateId(),
                Name = $"Time: {DateTime.Now}"
            };

            var currState = _mapper.Map<DummyDTO>(dbObj);
            dbObj.AddMasterEvent(_gen.GenerateId(), command, null, currState);

            await _dummyRepository.AddAsync(dbObj);
            await _unitOfWork.SaveChangesAsync();

            return command.OkResponse(true);
        }
    }
}
