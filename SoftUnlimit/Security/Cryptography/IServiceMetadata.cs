using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.Security.Cryptography
{
    /// <summary>
    /// Service metadata.
    /// </summary>
    public interface IServiceMetadata
    {
        /// <summary>
        /// Service identifier.
        /// </summary>
        uint ServiceId { get; }
        /// <summary>
        /// Worker identifier.
        /// </summary>
        ushort WorkerId { get; }
    }
}
