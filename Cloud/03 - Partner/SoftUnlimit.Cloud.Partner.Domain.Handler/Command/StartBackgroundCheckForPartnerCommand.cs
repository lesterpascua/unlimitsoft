//using Hangfire;
//using SoftUnlimit.Cloud.Command;
//using SoftUnlimit.Cloud.Partner.Data;
//using SoftUnlimit.Cloud.Partner.Data.Model;
//using SoftUnlimit.Cloud.Security;
//using SoftUnlimit.CQRS.Command;
//using SoftUnlimit.CQRS.Message;
//using System;
//using System.Threading;
//using System.Threading.Tasks;

//namespace SoftUnlimit.Cloud.Partner.Domain.Handler.Command
//{
//    /// <summary>
//    /// Initialize scheduler backgound for partner.
//    /// </summary>
//    public sealed class StartBackgroundCheckForPartnerCommand : CloudCommand
//    {
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="id"></param>
//        /// <param name="user"></param>
//        public StartBackgroundCheckForPartnerCommand(Guid id, IdentityInfo user)
//            : base(id, user)
//        {
//        }

//        public PartnerValues PartnerId { get; set; }
//    }

//    public sealed class StartBackgroundCheckForPartnerCommandHandler :
//        ICloudCommandHandler<StartBackgroundCheckForPartnerCommand>
//    {
//        private readonly IBackgroundJobClient _jobClient;
//        private readonly ICloudUnitOfWork _unitOfWork;
//        private readonly ICloudRepository<PartnerJobs> _partnerJobsRepository;

//        public StartBackgroundCheckForPartnerCommandHandler(
//            IBackgroundJobClient jobClient,
//            ICloudUnitOfWork unitOfWork,
//            ICloudRepository<PartnerJobs> partnerJobsRepository)
//        {
//            _jobClient = jobClient;
//            _unitOfWork = unitOfWork;
//            _partnerJobsRepository = partnerJobsRepository;
//        }

//        public async Task<ICommandResponse> HandleAsync(StartBackgroundCheckForPartnerCommand command, CancellationToken ct = default)
//        {
//            await Task.CompletedTask;
//            return command.OkResponse();
//        }
//    }
//}
