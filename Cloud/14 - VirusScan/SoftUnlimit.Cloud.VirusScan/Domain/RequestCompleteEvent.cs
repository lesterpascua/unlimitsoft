using SoftUnlimit.Cloud.Antivirus;
using SoftUnlimit.Cloud.Event;
using SoftUnlimit.Cloud.Storage;
using System;

namespace SoftUnlimit.Cloud.VirusScan.Domain
{
    /// <summary>
    /// All items requested to scanned.
    /// </summary>
    /// <param name="CustomerId">Customer owner of the request, null if no customer asociate.</param>
    /// <param name="RequestId">External identifier of the request. This id will be use as documentId</param>
    /// <param name="BlobUri">External storage path of the request.</param>
    /// <param name="Metadata">Extra information of the request will be pass to the next step in the process.</param>
    /// <param name="DownloadStatus">Status when the system try to download the file.</param>
    /// <param name="ScanStatus">Status when the system try to scan the file.</param>
    /// <param name="Success">Indicate if the operation is considered success or not.</param>
    public record RequestCompleteBody(Guid? CustomerId, Guid? RequestId, string BlobUri, object Metadata, StorageStatus DownloadStatus, ScanStatus ScanStatus, bool Success);
    /// <summary>
    /// Notify the request is complete and the status of the request.
    /// </summary>
    public sealed class RequestCompleteEvent : CloudEvent<RequestCompleteBody>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sourceId"></param>
        /// <param name="version"></param>
        /// <param name="serviceId"></param>
        /// <param name="workerId"></param>
        /// <param name="correlationId"></param>
        /// <param name="command"></param>
        /// <param name="prevState"></param>
        /// <param name="currState"></param>
        /// <param name="isDomainEvent"></param>
        /// <param name="body"></param>
        public RequestCompleteEvent(Guid id, Guid sourceId, long version, ushort serviceId, string workerId, string correlationId, object command, object prevState, object currState, bool isDomainEvent, RequestCompleteBody body) :
            base(id, sourceId, version, serviceId, workerId, correlationId, command, prevState, currState, isDomainEvent, body)
        {
        }
    }
}
