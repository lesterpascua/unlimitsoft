using SoftUnlimit.Cloud.Event;
using System;

namespace SoftUnlimit.Cloud.VirusScan.Domain
{
    /// <summary>
    /// Body for the create request event.
    /// </summary>
    /// <param name="Requests">Array of the request.</param>
    public record RequestCreateBody(RequestInfo[] Requests);
    /// <summary>
    /// All items requested to scanned.
    /// </summary>
    /// <param name="CustomerId">Customer owner of the request, null if no customer asociate.</param>
    /// <param name="RequestId">External identifier of the request. This id will be use as documentId</param>
    /// <param name="BlobUri">External storage path of the request.</param>
    /// <param name="Metadata">Extra information of the request will be pass to the next step in the process.</param>
    public record RequestInfo(Guid? CustomerId, Guid? RequestId, string BlobUri, object Metadata = null);

    /// <summary>
    /// Allow create a new request of processing file.
    /// </summary>
    public sealed class RequestCreateEvent : CloudEvent<RequestCreateBody>
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
        public RequestCreateEvent(Guid id, Guid sourceId, long version, ushort serviceId, string workerId, string correlationId, object command, object prevState, object currState, bool isDomainEvent, RequestCreateBody body) : 
            base(id, sourceId, version, serviceId, workerId, correlationId, command, prevState, currState, isDomainEvent, body)
        {
        }
    }
}
