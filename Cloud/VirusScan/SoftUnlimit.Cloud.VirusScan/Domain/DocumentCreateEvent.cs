using SoftUnlimit.Cloud.Event;
using System;

namespace SoftUnlimit.Cloud.VirusScan.Domain
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="CustomerId">Customer owner of the document.</param>
    /// <param name="DocumentId">Document identifier. If is null this will generate in the document manager.</param>
    /// <param name="BlobUri">BlobUri of the document.</param>
    /// <param name="Metadata">Metadata asociate to the document.</param>
    public record DocumentCreateBody(Guid? CustomerId, Guid? DocumentId, string BlobUri, object Metadata = null);

    /// <summary>
    /// Triger when some document need to upload to onbase. This event is used in the file scanning to upload some event
    /// </summary>
    public sealed class DocumentCreateEvent : CloudEvent<DocumentCreateBody>
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
        public DocumentCreateEvent(Guid id, Guid sourceId, long version, ushort serviceId, string workerId, string correlationId, object command, object prevState, object currState, bool isDomainEvent, DocumentCreateBody body) :
            base(id, sourceId, version, serviceId, workerId, correlationId, command, prevState, currState, isDomainEvent, body)
        {
        }
    }
}
