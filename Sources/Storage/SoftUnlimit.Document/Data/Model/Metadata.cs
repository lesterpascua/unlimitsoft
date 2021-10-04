using SoftUnlimit.CQRS.EventSourcing;
using SoftUnlimit.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.Document.Data.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class Metadata : EventSourced<Guid>
    {
        /// <summary>
        /// Name of file.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// File folder.
        /// </summary>
        public string Folder { get; set; }

        /// <summary>
        /// Get factory object GUID how interact with cloud document.
        /// </summary>
        public Guid Provider { get; set; }
        /// <summary>
        /// Link in the cloud
        /// </summary>
        public string CloudLink { get; set; }
        /// <summary>
        /// File mime tipe
        /// </summary>
        public string MimeType { get; set; }
        /// <summary>
        /// File size
        /// </summary>
        public long Length { get; set; }
        /// <summary>
        /// Expiration time.
        /// </summary>
        public DateTime? Expire { get; set; }
    }
}
