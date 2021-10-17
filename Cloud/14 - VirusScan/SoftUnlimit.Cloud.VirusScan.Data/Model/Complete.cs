using SoftUnlimit.Cloud.Antivirus;
using SoftUnlimit.Cloud.Event;
using SoftUnlimit.Cloud.Security.Cryptography;
using SoftUnlimit.Cloud.Storage;
using SoftUnlimit.CQRS.EventSourcing;
using System;

namespace SoftUnlimit.Cloud.VirusScan.Data.Model
{
    /// <summary>
    /// All pending request in the queue.
    /// </summary>
    public class Complete : EventSourced<Guid>
    {
        /// <summary>
        /// User owner of the file. Null if no user asociate.
        /// </summary>
        public Guid? CustomerId { get; set; }
        /// <summary>
        /// Identifier of the request.
        /// </summary>
        public Guid? RequestId { get; set; }
        /// <summary>
        /// CorrelationId asociate to the process. Unique value to identifier the source of the operation.
        /// </summary>
        public string CorrelationId { get; set; }
        /// <summary>
        /// Unique BlobUri identifier of the file.
        /// </summary>
        public string BlobUri { get; set; }
        /// <summary>
        /// Indicate scanning status of the file (allow better trace of the error).
        /// </summary>
        public ScanStatus ScanStatus { get; set; }
        /// <summary>
        /// Indicate download status of the file (allow better trace of the error).
        /// </summary>
        public StorageStatus DownloadStatus { get; set; }
        /// <summary>
        /// Date where request is created
        /// </summary>
        public DateTime Created { get; set; }
        /// <summary>
        /// Date when the file was scanned 
        /// </summary>
        public DateTime Scanned { get; set; }
        /// <summary>
        /// Number of retry attemp for the file
        /// </summary>
        public int Retry { get; set; }

        /// <summary>
        /// Reference to customer table.
        /// </summary>
        public virtual Customer Customer { get; set; }

        /// <summary>
        /// Add versioned event.
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="gen"></param>
        /// <param name="correlationId"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public IVersionedEvent AddEvent(Type eventType, Guid? identityId, ICloudIdGenerator gen, string correlationId, object body)
        {
            var @event = AddVersionedEvent(eventType, gen.GenerateId(), gen.ServiceId, gen.WorkerId, correlationId, body);
            if (identityId is not null)
                ((CloudEvent)@event).IdentityId = identityId;

            return @event;
        }
    }
}
