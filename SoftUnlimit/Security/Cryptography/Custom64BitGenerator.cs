using SoftUnlimit.Properties;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SoftUnlimit.Security.Cryptography
{
    /// <summary>
    /// 
    /// </summary>
    public class Custom64BitGenerator : IIdGenerator<ulong>
    {
        private const ulong Mask = 18446744073709551615L;       // -1L

        private readonly object _sync;                          // Object used as a monitor for threads synchronization.
        private readonly DateTime _startEpoch;
        private readonly int _timestampLeftShift;
        private readonly int _sequenceBits, _identifierBits;
        private readonly ulong _maxSequence;
        
        private ulong _sequence;
        private ulong _lastTimestamp = 0;                       // The timestamp used to generate last id by the worker


        /// <summary>
        /// Use default setting with, 10 IdentifierBits, 12 SequenceBits, StartEpoch one year in the past.
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="sequence"></param>
        public Custom64BitGenerator(uint identifier = 0, uint sequence = 0)
            : this(new CustomGeneratorSettings {
                IdentifierBits = 10,
                SequenceBits = 12,
                StartEpoch = DateTime.UtcNow.AddYears(-1)
            }, identifier, sequence)
        {
        }
        /// <summary>
        /// Use create a new instance of 64 bit generator.
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="sequence"></param>
        /// <param name="settings"></param>
        public Custom64BitGenerator(CustomGeneratorSettings settings, uint identifier = 0, uint sequence = 0)
        {
            this._maxSequence = Mask ^ (Mask << settings.SequenceBits);
            ulong MaxIdentifierBits = Mask ^ (Mask << settings.IdentifierBits);

            if (sequence > this._maxSequence || sequence < 0)
                throw new InvalidOperationException(string.Format(Resources.InvalidOperationException_PropertyBetween, "Sequence", this._maxSequence, 0));
            if (identifier > MaxIdentifierBits || identifier < 0)
                throw new InvalidOperationException(string.Format(Resources.InvalidOperationException_PropertyBetween, "Identifier", MaxIdentifierBits, 0));

            this._sequence = sequence;
            this.Identifier = identifier;

            this._sync = new object();
            this._startEpoch = settings.StartEpoch;
            this._sequenceBits = settings.SequenceBits;
            this._identifierBits = settings.IdentifierBits;

            this._timestampLeftShift = this._identifierBits + this._sequenceBits;
        }

        /// <summary>
        /// 
        /// </summary>
        public uint Identifier { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ulong GenerateId()
        {
            lock(this._sync)
                return this.NextId();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator<ulong> GetEnumerator()
        {
            while (true)
                yield return GenerateId();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #region Private Properties

        private ulong CurrentTime
        {
            get { return (ulong)(DateTime.UtcNow - this._startEpoch).TotalMilliseconds; }
        }

        #endregion

        #region Private Methods

        private ulong NextId()
        {
            var timestamp = this.CurrentTime;
            if (timestamp < this._lastTimestamp)
                throw new InvalidOperationException(string.Format(Resources.InvalidOperationException_ClockMovedBackwards, (this._lastTimestamp - timestamp)));

            if (this._lastTimestamp == timestamp)
            {
                if (++this._sequence > this._maxSequence)
                    timestamp = this.TillNextMillis(this._lastTimestamp);
            } else
                this._sequence = 0;

            this._lastTimestamp = timestamp;
            return (timestamp << this._timestampLeftShift) | (this.Identifier << this._identifierBits) | this._sequence;
        }
        private ulong TillNextMillis(ulong lastTimestamp)
        {
            var timestamp = this.CurrentTime;
            SpinWait.SpinUntil(() => (timestamp = CurrentTime) > lastTimestamp);

            return timestamp;
        }

        #endregion
    }
}
