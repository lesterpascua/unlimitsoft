using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftUnlimit.Bus.Hangfire;
using SoftUnlimit.Cloud.Antivirus;
using SoftUnlimit.Cloud.Command;
using SoftUnlimit.Cloud.Security;
using SoftUnlimit.Cloud.Security.Cryptography;
using SoftUnlimit.Cloud.Storage;
using SoftUnlimit.Cloud.VirusScan.Data;
using SoftUnlimit.Cloud.VirusScan.Data.Model;
using SoftUnlimit.Cloud.VirusScan.Domain.Handler.Configuration;
using SoftUnlimit.Cloud.VirusScan.Logging;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Command.Validation;
using SoftUnlimit.CQRS.Message;
using SoftUnlimit.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.Cloud.VirusScan.Domain.Handler.Command
{
    /// <summary>
    /// Triger virus scan process.
    /// </summary>
    public sealed class ScanRequestCommand : CloudCommand, ISchedulerCommand
    {
        public ScanRequestCommand(Guid id, IdentityInfo user, Guid pendingId) :
            base(id, user)
        {
            RequestId = pendingId;
        }

        /// <summary>
        /// Identifier of the pending request.
        /// </summary>
        public Guid RequestId { get; }
        /// <summary>
        /// Delay time of the command. Usefull to delay the execution of the command in the time.
        /// </summary>
        public TimeSpan? Delay { get; set; }
    }

    public sealed class ScanRequestCommandHandler : 
        ICommandHandlerValidator<ScanRequestCommand>,
        ICloudCommandHandler<ScanRequestCommand>
    {
        private Pending _request;
        private readonly ICommandBus _bus;
        private readonly ICloudIdGenerator _gen;
        private readonly IVirusScanService _scanService;
        private readonly IExternalStorage _externalStorage;
        private readonly ICloudUnitOfWork _unitOfWork;
        private readonly ICloudRepository<Pending> _pendingRepository;
        private readonly ICloudRepository<Complete> _completeRepository;
        private readonly AntivirusOptions _options;
        private readonly ILogger<ScanRequestCommandHandler> _logger;


        public ScanRequestCommandHandler(
            ICommandBus bus,
            ICloudIdGenerator gen,
            IVirusScanService scanService,
            IExternalStorage externalStorage,
            ICloudUnitOfWork unitOfWork, 
            ICloudRepository<Pending> pendingRepository,
            ICloudRepository<Complete> completeRepository,
            IOptions<AntivirusOptions> options,
            ILogger<ScanRequestCommandHandler> logger)
        {
            _bus = bus;
            _gen = gen;
            _scanService = scanService;
            _externalStorage = externalStorage;
            _unitOfWork = unitOfWork;
            _pendingRepository = pendingRepository;
            _completeRepository = completeRepository;
            _options = options.Value;
            _logger = logger;
        }

        /// <summary>
        /// Validator for file scanning.
        /// </summary>
        /// <param name="validator"></param>
        /// <returns></returns>
        public IValidator BuildValidator(CommandValidator<ScanRequestCommand> validator)
        {
            validator.RuleFor(p => p.RequestId)
                .MustAsync(async (id, ct) =>
                {
                    _request = await _pendingRepository
                        .FindAll()
                        .Include(i => i.Customer)
                        .FirstOrDefaultAsync(p => p.Id == id, ct);
                    return _request != null;
                }).WithMessage($"{WellKnownError.BadRequest_RequestMustExist:D}")

                .Must(id => _request.Status == Client.StatusValues.Approved).WithMessage($"{WellKnownError.BadRequest_RequestInvalidStatus:D}");

            return validator;
        }
        /// <summary>
        /// Business logic to scan file.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<ICommandResponse> HandleAsync(ScanRequestCommand command, CancellationToken ct = default)
        {
            try
            {
                if (_request.Customer is null || _request.Customer.VirusDetected < _options.MaxVirusAttemptPerCustomer)
                {
                    await ScanRequest(command, ct);
                }
                else
                    await MoveToCompleteAsync(StorageStatus.Success, ScanStatus.VirusDetected, true, ct);
                return command.OkResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing {Id}", _request.Id);

                await ReSchedulerRequest(command, ct);
                await _unitOfWork.SaveChangesAsync(ct);
                
                return command.AceptedResponse();
            }
        }

        #region Private Methods
        private async Task<bool> ScanRequest(ScanRequestCommand command, CancellationToken ct)
        {
            ScanStatus scanStatus;
            var (raw, downloadStatus) = await _externalStorage.DownloadAsync(_request.BlobUri, StorageType.Pending, ct);
            //
            // check if some error happened in the download and scan file.
            switch (downloadStatus)
            {
                case StorageStatus.ServiceOffline:
                    scanStatus = ScanStatus.ServiceOffline;
                    _logger.LogWarning("Service offline when scanning {Id}", _request.Id);
                    break;
                case StorageStatus.Success:
                    scanStatus = await _scanService.ScanAsync(_request.BlobUri, raw, ct);
                    _logger.LogDebug("Success scanning {Id}", _request.Id);
                    break;
                case StorageStatus.NotFound:
                    scanStatus = ScanStatus.Error;
                    _logger.LogWarning("Not found file {Id}", _request.Id);
                    break;
                case StorageStatus.Error:
                default:
                    scanStatus = ScanStatus.Error;
                    _logger.LogError("Error scanning file {Id}", _request.Id);
                    break;
            };

            var success = false;
            switch (scanStatus)
            {
                case ScanStatus.Clean:
                    await MarkAttemptAsCleanAsync(command, downloadStatus, scanStatus, ct);
                    success = true;
                    break;
                case ScanStatus.VirusDetected:
                    await MarkAttemptAsVirusAsync(downloadStatus, scanStatus, ct);
                    break;
                case ScanStatus.ServiceOffline:
                    await ReSchedulerRequest(command, ct);
                    break;
                case ScanStatus.Error:
                case ScanStatus.Unknown:
                default:
                    await MarkAttemptAsErrorAsync(downloadStatus, scanStatus, ct);
                    break;
            }
            await _unitOfWork.SaveChangesAsync(ct);
            return success;
        }

        private async Task MarkAttemptAsErrorAsync(StorageStatus downloadStatus, ScanStatus scanStatus, CancellationToken ct)
        {
            _logger.LogError("Error scanning file {Id}", _request.Id);

            await MoveToCompleteAsync(downloadStatus, scanStatus, true, ct);
        }
        private async Task MarkAttemptAsVirusAsync(StorageStatus downloadStatus, ScanStatus scanStatus, CancellationToken ct)
        {
            var customer = _request.Customer;
            if (customer.FirstVirusDetected is null)
                customer.FirstVirusDetected = DateTime.UtcNow;
            customer.VirusDetected += 1;

            if (_options.MaxVirusAttemptPerCustomer <= customer.VirusDetected)
            {
                // notify customer
                await Task.CompletedTask;
            }

            await MoveToCompleteAsync(downloadStatus, scanStatus, true, ct);
        }
        private async Task MarkAttemptAsCleanAsync(ScanRequestCommand command, StorageStatus downloadStatus, ScanStatus scanStatus, CancellationToken ct)
        {
            var (success, _) = await _externalStorage.MoveAsync(_request.BlobUri, StorageType.Pending, StorageType.Clean, ct);
            if (success != false)
            {
                _logger.LogDebug("Send notificaton event to document management for {Id}", _request.Id);

                var (complete, metadata) = await MoveToCompleteAsync(downloadStatus, scanStatus, false, ct);

                var documentCreateBody = new DocumentCreateBody(_request.CustomerId, _request.RequestId, _request.BlobUri, metadata);
                complete.AddEvent(typeof(DocumentCreateEvent), _gen, _request.CorrelationId, documentCreateBody);
            }
            else
            {
                _logger.LogWarning("Error moving blob from {Pending} to {Clean}", StorageType.Pending, StorageType.Clean);

                await ReSchedulerRequest(command, ct);
            }
        }

        /// <summary>
        /// ReScheculer the process to later for some service is offline. This implied push the operation in the CommandBus.
        /// </summary>
        private async Task ReSchedulerRequest(ScanRequestCommand command, CancellationToken ct)
        {
            command.Delay = TimeSpanUtility.DuplicateRetryTime(command.Delay);

            _request.Retry += 1;
            _request.Scheduler = DateTime.UtcNow.Add(command.Delay.Value);

            await _bus.SendAsync(command, ct);
        }
        /// <summary>
        /// Delete BlobUri or open a task to delete in background.
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task DeleteBlobUriAsync(CancellationToken ct)
        {
            var (deleted, reason) = await _externalStorage.DeleteAsync(_request.BlobUri, StorageType.Pending, ct);

            // if not deleted check the reason to delete in the future
            if (!deleted)
            {
                _logger.LogWarning("Error deleting {BlobUri}", _request.BlobUri);
                switch (reason)
                {
                    case StorageStatus.NotFound:
                    case StorageStatus.Error:
                    case StorageStatus.ServiceOffline:
                        break;
                }
            }
        }
        /// <summary>
        /// Move the request to the complete table.
        /// </summary>
        /// <param name="downloadStatus"></param>
        /// <param name="scanStatus"></param>
        /// <param name="deleteBlobUri"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task<(Complete, object)> MoveToCompleteAsync(StorageStatus downloadStatus, ScanStatus scanStatus, bool deleteBlobUri, CancellationToken ct)
        {
            _pendingRepository.Remove(_request);

            var complete = new Complete
            {
                BlobUri = _request.BlobUri,
                CorrelationId = _request.CorrelationId,
                Created = _request.Created,
                CustomerId = _request.CustomerId,
                ScanStatus = scanStatus,
                DownloadStatus = downloadStatus,
                RequestId = _request.RequestId,
                Id = _request.Id,
                Retry = _request.Retry,
                Scanned = DateTime.UtcNow
            };
            await _completeRepository.AddAsync(complete, ct);
            if (deleteBlobUri)
                await DeleteBlobUriAsync(ct);

            var metadata = JsonUtility.Deserialize<object>(_request.Metadata);
            var body = new RequestCompleteBody(_request.CustomerId, _request.RequestId, _request.BlobUri, metadata, downloadStatus, scanStatus, false);
            complete.AddEvent(typeof(RequestCompleteEvent), _gen, _request.CorrelationId, body);

            return (complete, metadata);
        }
        #endregion
    }
}
