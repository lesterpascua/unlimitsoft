using SoftUnlimit.Cloud.Command;
using SoftUnlimit.Cloud.Security;
using SoftUnlimit.Cloud.Security.Cryptography;
using SoftUnlimit.Cloud.VirusScan.Client;
using SoftUnlimit.Cloud.VirusScan.Data;
using SoftUnlimit.Cloud.VirusScan.Data.Model;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Message;
using SoftUnlimit.Json;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.Cloud.VirusScan.Domain.Handler.Command
{
    /// <summary>
    /// Save the request in database to process the file in the future.
    /// </summary>
    public sealed class CreateRequestCommand : CloudCommand
    {
        public CreateRequestCommand(Guid id, IdentityInfo user, RequestCreateBody eventBody) :
            base(id, user)
        {
            Requests = eventBody.Requests;
        }

        /// <summary>
        /// Collection of request data for the recuest.
        /// </summary>
        public RequestInfo[] Requests { get; set; }
    }
    /// <summary>
    /// Bussiness logic asociate to <see cref="CreateRequestCommand"/>
    /// </summary>
    public sealed class CreateRequestCommandHandler :
        ICloudCommandHandler<CreateRequestCommand>
    {
        private readonly ICloudIdGenerator _gen;
        private readonly ICloudUnitOfWork _unitOfWork;
        private readonly ICloudRepository<Pending> _pendingRepository;
        private readonly ICommandBus _commandBus;


        public CreateRequestCommandHandler(
            ICloudIdGenerator gen,
            ICloudUnitOfWork unitOfWork, 
            ICloudRepository<Pending> pendingRepository,
            ICommandBus commandBus)
        {
            _gen = gen;
            _unitOfWork = unitOfWork;
            _pendingRepository = pendingRepository;
            _commandBus = commandBus;
        }

        public async Task<ICommandResponse> HandleAsync(CreateRequestCommand command, CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            var user = command.Props.User;
            var pendings = command
                .Requests
                .Select(r => new Pending
                {
                    BlobUri = r.BlobUri,
                    CorrelationId = user.CorrelationId,
                    Created = now,
                    CustomerId = r.CustomerId,
                    Id = _gen.GenerateId(),
                    Metadata = r.Metadata is not null ? JsonUtility.Serialize(r.Metadata) : null,
                    RequestId = r.RequestId,
                    Retry = 0,
                    Scheduler = now,
                    Status = StatusValues.Approved,
                });
            await _pendingRepository.AddRangeAsync(pendings, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var tasks = pendings
                .Select(s => _commandBus.SendAsync(new ScanRequestCommand(_gen.GenerateId(), command.Props.User, s.Id), ct))
                .ToArray();
            Task.WaitAll(tasks, ct);

            return command.OkResponse();
        }
    }
}