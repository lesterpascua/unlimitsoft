using SoftUnlimit.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
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
    public class IdGuidGenerator : IIdGenerator<Guid>, IServiceMetadata
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
        /// <param name="nic">MAC addresss identifier.</param>
        public IdGuidGenerator(PhysicalAddress nic)
            : this(nic.GetAddressBytes(), CustomGeneratorSettings.Jan1st1970)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="nic">MAC addresss identifier.</param>
        public IdGuidGenerator(NetworkInterface nic)
            : this(nic.GetPhysicalAddress())
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
            _id0 = identifier[0]; _id1 = identifier[1];
            _id2 = identifier[2]; _id3 = identifier[3];
            _id4 = identifier[4]; _id5 = identifier[5];

            Id = Convert.ToBase64String(identifier);
            WorkerId = (ushort)(_id0 << (8 * 0) | _id1 << (8 * 1));
            ServiceId = (uint)(_id2 << (8 * 0) | _id3 << (8 * 1) | _id4 << (8 * 2) | _id5 << (8 * 3));
        }


        /// <summary>
        /// 
        /// </summary>
        public string Id { get; }
        /// <summary>
        /// Service identifier.
        /// </summary>
        public uint ServiceId { get; }
        /// <summary>
        /// Worker identifier.
        /// </summary>
        public ushort WorkerId { get; }

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

        #endregion Public Methods

        #region Private Methods

        private ulong CurrentTime => (ulong)(DateTime.UtcNow - this._startEpoch).Ticks;

        private Guid NextId()
        {
            var timestamp = CurrentTime;
            if (timestamp < _lastTimestamp)
                throw new InvalidOperationException(string.Format(Resources.InvalidOperationException_ClockMovedBackwards, (_lastTimestamp - timestamp)));

            if (_lastTimestamp == timestamp)
            {
                if (++_sequence > ushort.MaxValue)
                    timestamp = TillNextMillis(_lastTimestamp);
            } else
                _sequence = 0;

            _lastTimestamp = timestamp;
            return new Guid(
                (int)(_lastTimestamp >> 32 & 0xFFFFFFFF),
                (short)(_lastTimestamp >> 16 & 0xFFFF),
                (short)(_lastTimestamp & 0xFFFF),
                _id5, _id4, _id3, _id2, _id1, _id0,
                (byte)(_sequence >> 8 & 0xFF),
                (byte)(_sequence >> 0 & 0xFF));
        }
        private ulong TillNextMillis(ulong lastTimestamp)
        {
            ulong timestamp = this.CurrentTime;
            SpinWait.SpinUntil(() => (timestamp = this.CurrentTime) > lastTimestamp);

            return timestamp;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte[] CreateIdentifier(uint service, ushort worker) => new byte[6] {
            (byte)(worker >> (8 * 0) & 0xff),
            (byte)(worker >> (8 * 1) & 0xff),

            (byte)(service >> (8 * 0) & 0xff),
            (byte)(service >> (8 * 1) & 0xff),
            (byte)(service >> (8 * 2) & 0xff),
            (byte)(service >> (8 * 3) & 0xff),
        };

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion Private Methods

        /// <summary>
        /// Get first valid network interface to get the mack address as identifier.
        /// </summary>
        /// <param name="id">Prefered network identifier.</param>
        /// <returns></returns>
        public static NetworkInterface GetNetworkInterface(string id = null)
        {
            var query = NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(
                    nic => nic.OperationalStatus == OperationalStatus.Up && 
                    nic.NetworkInterfaceType != NetworkInterfaceType.Loopback && 
                    nic.GetPhysicalAddress().GetAddressBytes()?.Length == 6);
            if (!string.IsNullOrEmpty(id))
                return query.First(p => p.Id == id);

            return query.First();
        }
    }
}
