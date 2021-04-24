using App.Manual.Tests.CQRS.Data;
using FluentValidation;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Command.Validation;
using SoftUnlimit.CQRS.Message;
using SoftUnlimit.Data;
using SoftUnlimit.Map;
using SoftUnlimit.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace App.Manual.Tests.CQRS.Command
{
    public class DummyCommandHandler :
        ICommandHandlerValidator,
        ICommandHandlerCompliance,
        ICommandHandler<DummyCreateCommand>
    {
        private readonly IIdGenerator<Guid> _gen;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Dummy> _dummyRepository;

        public DummyCommandHandler(
            IIdGenerator<Guid> gen,
            IDbUnitOfWork unitOfWork,
            IDbRepository<Dummy> dummyRepository
        )
        {
            _gen = gen;
            _unitOfWork = unitOfWork;
            _dummyRepository = dummyRepository;
        }

        public IValidator BuildValidator(IValidator v) => v switch
        {
            CommandValidator<DummyCreateCommand> validator => BuildValidator(validator),
            _ => throw new NotSupportedException("This handler not implement this validator"),
        };

        private IValidator BuildValidator(CommandValidator<DummyCreateCommand> validator)
        {
            validator.RuleFor(p => p.Name)
                .Must(name =>
                {
                    return name.Length > 3;
                });
            return validator;
        }

        public async Task<CommandResponse> HandleAsync(DummyCreateCommand command, object validationCache)
        {
            var dbObj = new Dummy {
                Id = _gen.GenerateId(),
                Name = $"Time: {DateTime.Now}"
            };

            dbObj.AddMasterEvent(_gen.GenerateId(), "asdsadasd", command, null, dbObj);

            await _dummyRepository.AddAsync(dbObj);
            await _unitOfWork.SaveChangesAsync();

            return command.OkResponse(true);
        }

        public Task<CommandResponse> HandleComplianceAsync(ICommand command)
        {
            return Task.FromResult(command.OkResponse(true));
        }
    }
}
