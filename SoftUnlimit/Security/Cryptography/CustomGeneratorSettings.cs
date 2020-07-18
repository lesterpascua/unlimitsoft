using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.Security.Cryptography
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class CustomGeneratorSettings
    {
        /// <summary>
        /// Amount of bit dedicate to the identifier. You can separate in datacenter | worker.
        /// </summary>
        public byte IdentifierBits { get; set; } = 10;
        /// <summary>
        /// Amount of bit dedicate to the secuence. Sequence is the maximun identifier allowed to generate by 1 milisecond.
        /// </summary>
        public byte SequenceBits { get; set; } = 12;
        /// <summary>
        /// Time used as begin of counter, by default is the Unix Epoch timestamp.
        /// </summary>
        public DateTime StartEpoch { get; set; } = Jan1st1970;

        /// <summary>
        /// 1 January 1970. Used to calculate timestamp (in milliseconds)
        /// </summary>
        public static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }
}
