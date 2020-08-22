using SoftUnlimit.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace SoftUnlimit.Security.Cryptography
{
    /// <summary>
    /// A decentralized, k-ordered id generator. Generated ids are Guid (128-bit wide)
    /// <list>
    ///     <item><description>64-bit timestamp - milliseconds since the epoch (Jan 1 1970)</description></item>
    ///     <item><description>32-bit service</description></item>
    ///     <item><description>16-bit worker id</description></item>
    ///     <item><description>16-bit secuence</description></item>
    /// </list>
    /// </summary>
    public class IdGuidGenerator : IIdGenerator<Guid>
    {
        private readonly DateTime _startEpoch;
        private readonly object _sync = new object();               // Object used as a monitor for threads synchronization.

        private int _sequence;                                      // The sequence within the same tick.
        private ulong _lastTimestamp;
        private readonly byte _id0, _id1, _id2, _id3, _id4, _id5;   // store the individual bytes instead of an array so we do not incur the overhead of array indexing and bound checks when generating id values


        /// <summary>
        /// 
        /// </summary>
        /// <param name="identifier">Identifier can be a mac addresss.</param>
        public IdGuidGenerator(byte[] identifier)
            : this(identifier, CustomGeneratorSettings.Jan1st1970)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="service">32 bit identifier.</param>
        /// <param name="worker">16 bit workey.</param>
        public IdGuidGenerator(uint service = 0, ushort worker = 0)
            : this(CreateIdentifier(service, worker), CustomGeneratorSettings.Jan1st1970)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="identifier">Identifier can be a mac addresss.</param>
        /// <param name="epoch"></param>
        public IdGuidGenerator(byte[] identifier, DateTime epoch)
        {
            if (identifier == null)
                throw new ArgumentNullException(nameof(identifier));
            if (identifier.Length != 6)
                throw new ArgumentException("Parameter must be an array of length equal 6.", nameof(identifier));

            _startEpoch = epoch;
            Id = Convert.ToBase64String(identifier);
            _id0 = identifier[0]; _id1 = identifier[1];
            _id2 = identifier[2]; _id3 = identifier[3];
            _id4 = identifier[4]; _id5 = identifier[5];
        }


        /// <summary>
        /// 
        /// </summary>
        public string Id { get; }

        #region Public Methods

        /// <summary>
        /// Generates new identifier every time the method is called
        /// </summary>
        /// <returns> The new generated identifier. </returns>
        public Guid GenerateId()
        {
            lock (this._sync)
                return this.NextId();
        }
        /// <summary>
        /// Get next identifier.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Guid> GetEnumerator()
        {
            while (true)
                yield return GenerateId();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString() => this.Id;
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion Public Methods

        #region Private Methods

        private ulong CurrentTime
        {
            get { return (ulong)(DateTime.UtcNow - this._startEpoch).Ticks; }
        }

        private Guid NextId()
        {
            var timestamp = this.CurrentTime;
            if (timestamp < this._lastTimestamp)
                throw new InvalidOperationException(string.Format(Resources.InvalidOperationException_ClockMovedBackwards, (this._lastTimestamp - timestamp)));

            if (this._lastTimestamp == timestamp)
            {
                if (++this._sequence > UInt16.MaxValue)
                    timestamp = this.TillNextMillis(this._lastTimestamp);
                Console.WriteLine(new Guid(
                (int)(this._lastTimestamp >> 32 & 0xFFFFFFFF),
                (short)(this._lastTimestamp >> 16 & 0xFFFF),
                (short)(this._lastTimestamp & 0xFFFF),
                this._id5, this._id4, this._id3, this._id2, this._id1, this._id0,
                (byte)(this._sequence >> 8 & 0xFF),
                (byte)(this._sequence >> 0 & 0xFF)));
            } else
                this._sequence = 0;

            this._lastTimestamp = timestamp;
            return new Guid(
                (int)(this._lastTimestamp >> 32 & 0xFFFFFFFF),
                (short)(this._lastTimestamp >> 16 & 0xFFFF),
                (short)(this._lastTimestamp & 0xFFFF),
                this._id5, this._id4, this._id3, this._id2, this._id1, this._id0,
                (byte)(this._sequence >> 8 & 0xFF),
                (byte)(this._sequence >> 0 & 0xFF));
        }
        private ulong TillNextMillis(ulong lastTimestamp)
        {
            ulong timestamp = this.CurrentTime;
            SpinWait.SpinUntil(() => (timestamp = this.CurrentTime) > lastTimestamp);

            return timestamp;
        }
        private static byte[] CreateIdentifier(uint service, ushort worker)
        {
            return new byte[6] {
                (byte)(worker >> (8 * 0) & 0xff),
                (byte)(worker >> (8 * 1) & 0xff),

                (byte)(service >> (8 * 0) & 0xff),
                (byte)(service >> (8 * 1) & 0xff),
                (byte)(service >> (8 * 2) & 0xff),
                (byte)(service >> (8 * 3) & 0xff),
            };
        }

        #endregion Private Methods
    }
}
