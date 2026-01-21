using UnlimitSoft.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;

namespace UnlimitSoft.Security.Cryptography;


/// <summary>
/// A decentralized, k-ordered id generator. Generated ids are Guid (128-bit wide) compatible with RFC 9562 Version 7.
/// Allow create unique identifier for entities across microservices
/// </summary>
/// <remarks>
/// RFC 9562 Version 7 UUID structure with custom encoding:
/// <list>
///     <item>48-bit timestamp - milliseconds since the epoch (Jan 1 1970)</item>
///     <item>4-bit version (0111 = 7)</item>
///     <item>12-bit rand_a - contains 8 bits from worker ID byte 0 (_id0) + 4 bits from serviceId (bits 8-11)</item>
///     <item>2-bit variant (10)</item>
///     <item>62-bit rand_b - contains:
///         <list type="bullet">
///             <item>14 bits from serviceId (bits 0-7 and bits 12-15)</item>
///             <item>32 bits from worker ID bytes 1-4 (_id1, _id2, _id3, _id4)</item>
///             <item>16 bits from sequence counter</item>
///         </list>
///     </item>
/// </list>
/// 
/// Detailed bit allocation (Guid byte array representation):
/// - Bytes 0-5 (48 bits): Timestamp in milliseconds since epoch
/// - Bytes 6-7 (16 bits): Version (4 bits) + worker ID _id0 (8 bits) + serviceId bits 8-11 (4 bits)
/// - Bytes 8-11 (32 bits): Worker ID bytes 1-4 (_id1, _id2, _id3, _id4)
/// - Byte 12 (8 bits): Variant (2 bits) + serviceId bits 12-15 (6 bits, upper 2 bits used for bits 12-13, lower 4 bits used for bits 14-15)
/// - Byte 13 (8 bits): ServiceId bits 0-7 (8 bits)
/// - Bytes 14-15 (16 bits): Sequence counter
/// 
/// Worker ID: 40 bits (5 bytes) are preserved (_id0-_id4), _id5 is not encoded
/// ServiceId: Full 16 bits are preserved (bits 0-7 in byte 13, bits 8-11 in byte 6-7, bits 12-15 in byte 12)
/// </remarks>
public class MicroServiceGenerator : IIdGenerator<Guid>, IServiceMetadata
{
    private readonly ulong _startEpoch;

#if NET9_0_OR_GREATER
    private readonly Lock _sync = new();                        // Object used as a monitor for threads synchronization.
#else
    private readonly object _sync = new();                      // Object used as a monitor for threads synchronization.
#endif

    private int _sequence;                                      // The sequence within the same tick.
    private ulong _lastTimestamp;
    private readonly byte _id0, _id1, _id2, _id3, _id4, _id5;   // store the individual bytes instead of an array so we do not incur the overhead of array indexing and bound checks when generating id values


    /// <summary>
    /// Create generator using the MAC identifier.
    /// </summary>
    /// <param name="serviceId"></param>
    /// <param name="epoch">Epoc start time, by default linux epoch</param>
    public MicroServiceGenerator(ushort serviceId, DateTime? epoch = null)
        : this(serviceId, Utility.GetNetworkInterface(), epoch)
    {
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="serviceId"></param>
    /// <param name="workerId">Worker identifier can be a mac addresss. the identifier is array of 6 entries.</param>
    /// <param name="epoch">Epoc start time, by default linux epoch</param>
    public MicroServiceGenerator(ushort serviceId, byte[] workerId, DateTime? epoch = null)
    {
        epoch ??= CustomGeneratorSettings.Jan1st1970;

#if NETSTANDARD2_0
        if (workerId == null)
            throw new ArgumentNullException(nameof(workerId));
#else
        ArgumentNullException.ThrowIfNull(workerId, nameof(workerId));
#endif
        if (workerId.Length != 6)
            throw new ArgumentException("Parameter must be an array of length equal 6.", nameof(workerId));

        _startEpoch = (ulong)epoch.Value.Ticks / TimeSpan.TicksPerMillisecond;

        ServiceId = serviceId;
        WorkerId = Convert.ToBase64String(workerId);

        _id0 = workerId[0]; _id1 = workerId[1]; _id2 = workerId[2]; 
        _id3 = workerId[3]; _id4 = workerId[4]; _id5 = workerId[5];

        Id = Convert.ToBase64String([.. BitConverter.GetBytes(serviceId).Concat(workerId)]);
    }
    /// <summary>
    /// Create generator using the MAC identifier.
    /// </summary>
    /// <param name="serviceId"></param>
    /// <param name="nic">Phisical MAC addresss</param>
    /// <param name="epoch">Epoc start time, by default linux epoch</param>
    public MicroServiceGenerator(ushort serviceId, PhysicalAddress nic, DateTime? epoch = null)
        : this(serviceId, nic.GetAddressBytes(), epoch)
    {
    }
    /// <summary>
    /// Create generator using the network adapter.
    /// </summary>
    /// <param name="serviceId"></param>
    /// <param name="nic">Network adapter</param>
    /// <param name="epoch">Epoc start time, by default linux epoch</param>
    public MicroServiceGenerator(ushort serviceId, NetworkInterface nic, DateTime? epoch = null)
        : this(serviceId, nic.GetPhysicalAddress(), epoch)
    {
    }

    /// <summary>
    /// Generator identifier
    /// </summary>
    /// <remarks>
    /// Is calculete with the union of the serviceId and workerId coding in base64. Array(serviceId{2 bytes}, workerId{6 byte})
    /// </remarks>
    public string Id { get; }
    /// <summary>
    /// Worker identifier.
    /// </summary>
    public string WorkerId { get; }
    /// <summary>
    /// Service identifier.
    /// </summary>
    public ushort ServiceId { get; }

    /// <summary>
    /// Generates new identifier every time the method is called
    /// </summary>
    /// <returns> The new generated identifier. </returns>
    public Guid GenerateId()
    {
        lock (_sync)
            return NextId();
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
    public override string ToString() => Id;

    /// <summary>
    /// Extracts the sequence number from a Guid generated by MicroServiceGenerator.
    /// </summary>
    /// <param name="id">The Guid to extract the sequence from.</param>
    /// <returns>The sequence number encoded in the Guid.</returns>
    /// <remarks>
    /// The sequence is encoded in bytes 14-15 of the RFC 9562 Version 7 UUID (16 bits).
    /// Note: The rand_a field (bytes 6-7) contains worker ID data (_id0), not sequence data.
    /// </remarks>
    public static int GetSequence(Guid id)
    {
        var bytes = id.ToByteArray();

        // Bytes 14-15 contain the sequence counter (16 bits)
        return BitConverter.ToUInt16(bytes, 14);
    }
    /// <summary>
    /// Extracts the service identifier from a Guid generated by MicroServiceGenerator.
    /// </summary>
    /// <param name="id">The Guid to extract the service identifier from.</param>
    /// <returns>The service identifier encoded in the Guid.</returns>
    /// <remarks>
    /// The serviceId is encoded across bytes 6-7, 12, and 13 of the Guid's byte array representation:
    /// - Bytes 6-7 (rand_a): serviceId bits 8-11 (4 bits) in the lower 4 bits of the 12-bit rand_a field
    /// - Byte 12: variant (2 bits) + serviceId bits 12-15 (4 bits in lower nibble)
    /// - Byte 13: serviceId bits 0-7 (8 bits)
    /// This function reconstructs the full 16-bit serviceId from these bytes.
    /// </remarks>
    public static ushort GetServiceId(Guid id)
    {
        var bytes = id.ToByteArray();

        // Byte 13: Contains bits 0-7 of serviceId
        byte bits0to7 = bytes[13];

        // Bytes 6-7 (rand_a field): Extract bits 8-11 of serviceId
        // Encoding: short timeHiAndVersion = (short)(0x7000 | (_id0 << 4) | ((ServiceId >> 8) & 0x0F))
        // The lower 4 bits of this short contain serviceId bits 8-11
        // In little-endian byte array representation:
        // Byte 6 (low byte): Contains the lower 4 bits (serviceId bits 8-11)
        byte bits8to11 = (byte)(bytes[6] & 0x0F);

        // Byte 12: Extract bits 12-15 of serviceId (lower 4 bits after variant)
        // Encoding: byte clkSeqHiAndReserved = (byte)(0x80 | ((ServiceId >> 12) & 0x0F))
        byte bits12to15 = (byte)(bytes[12] & 0x0F);

        // Reconstruct the full 16-bit serviceId
        ushort serviceId = (ushort)(bits0to7 | (bits8to11 << 8) | (bits12to15 << 12));

        return serviceId;
    }
    /// <summary>
    /// Extracts the worker identifier from a Guid generated by MicroServiceGenerator.
    /// </summary>
    /// <param name="id">The Guid to extract the worker identifier from.</param>
    /// <returns>A partial 5-byte worker identifier encoded in the Guid (_id0-_id4).</returns>
    /// <remarks>
    /// The worker ID is encoded across multiple parts of the Guid's byte array representation:
    /// - Bytes 6-7 (rand_a): _id0 (8 bits) - bits 4-11 of the rand_a field (serviceId occupies bits 0-3)
    /// - Bytes 8-11: _id1, _id2, _id3, _id4 (32 bits)
    /// This function reconstructs 5 of the 6 bytes of the worker identifier.
    /// Note: _id5 is not preserved in the encoding.
    /// </remarks>
    public static byte[] GetWorkerId(Guid id)
    {
        var bytes = id.ToByteArray();

        // Extract _id1, _id2, _id3, _id4 from bytes 8-11
        byte id1 = bytes[8];
        byte id2 = bytes[9];
        byte id3 = bytes[10];
        byte id4 = bytes[11];

        // Extract _id0 from bytes 6-7
        // Encoding: short timeHiAndVersion = (short)(0x7000 | (_id0 << 4) | ((ServiceId >> 8) & 0x0F))
        // In little-endian byte array:
        // Byte 6 (low byte): serviceId bits 8-9 in bits 0-1, lower 4 bits of _id0 in bits 4-7
        // Byte 7 (high byte): version (0x7) in bits 4-7, upper 4 bits of _id0 in bits 0-3
        byte id0 = (byte)(((bytes[7] & 0x0F) << 4) | ((bytes[6] >> 4) & 0x0F));

        return [id0, id1, id2, id3, id4];
    }

    /// <summary>
    /// Extracts the timestamp from a Guid generated by MicroServiceGenerator.
    /// </summary>
    /// <param name="id">The Guid to extract the timestamp from.</param>
    /// <param name="epoch">The epoch used when the Guid was generated. If null, uses the default Linux epoch (Jan 1, 1970).</param>
    /// <returns>The timestamp as a DateTime in UTC.</returns>
    /// <remarks>
    /// The timestamp is encoded in the first 48 bits of the RFC 9562 Version 7 UUID:
    /// - Bytes 0-3: Most significant 32 bits of timestamp
    /// - Bytes 4-5: Next 16 bits of timestamp
    /// The timestamp represents milliseconds since the specified epoch.
    /// </remarks>
    public static DateTime GetTimestamp(Guid id, DateTime? epoch = null)
    {
        epoch ??= CustomGeneratorSettings.Jan1st1970;

        var bytes = id.ToByteArray();

        // Extract the 48-bit timestamp from bytes 0-5
        // Bytes 0-3: timeLow (most significant 32 bits of timestamp >> 16)
        var timeLow = BitConverter.ToInt32(bytes, 0);

        // Bytes 4-5: timeMid (next 16 bits of timestamp)
        var timeMid = BitConverter.ToInt16(bytes, 4);

        // Reconstruct the 48-bit timestamp
        // timeLow was created from (timestamp >> 16)
        // timeMid was created from (timestamp & 0xFFFF)
        var timestamp = ((ulong)(uint)timeLow << 16) | (ushort)timeMid;

        // Convert milliseconds since epoch to DateTime
        var epochMilliseconds = (ulong)epoch.Value.Ticks / TimeSpan.TicksPerMillisecond;
        var totalMilliseconds = epochMilliseconds + timestamp;
        var ticks = (long)(totalMilliseconds * TimeSpan.TicksPerMillisecond);

        return new DateTime(ticks, DateTimeKind.Utc);
    }

    #region Private Methods

    /// <summary>
    /// Current amount of miliseconds from start epoch
    /// </summary>
    private ulong CurrentTime => ((ulong)SysClock.GetUtcNow().Ticks / TimeSpan.TicksPerMillisecond) - _startEpoch;
    /// <summary>
    /// Get next identifier following RFC 9562 Version 7 format.
    /// </summary>
    /// <returns></returns>
    private Guid NextId()
    {
        var timestamp = CurrentTime;
        if (timestamp < _lastTimestamp)
            throw new InvalidOperationException(string.Format(Resources.InvalidOperationException_ClockMovedBackwards, (_lastTimestamp - timestamp)));

        if (_lastTimestamp == timestamp)
        {
            if (++_sequence > ushort.MaxValue)  // 16-bit max for the sequence
                timestamp = TillNextMillis(_lastTimestamp);
        }
        else
            _sequence = 0;

        _lastTimestamp = timestamp;

        // RFC 9562 Version 7 UUID layout (128 bits total):
        // 
        // 0                   1                   2                   3
        // 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
        // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        // |                          unix_ts_ms (32 bits)                 |
        // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        // |        unix_ts_ms (16)        |  ver  |     rand_a (12)       |
        // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        // |var|                    rand_b (62 bits)                       |
        // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        // |                         rand_b (cont)                         |
        // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        //
        // Custom encoding strategy:
        // - unix_ts_ms: 48-bit timestamp in milliseconds since epoch
        // - ver: 4-bit version = 0111 (7)
        // - rand_a (12 bits): _id0 (8 bits) + serviceId bits 8-11 (4 bits)
        // - var: 2-bit variant = 10
        // - rand_b (62 bits):
        //   * Bytes 8-11: _id1, _id2, _id3, _id4 (32 bits from worker ID)
        //   * Byte 12: variant (2 bits) + serviceId bits 12-15 (4 bits in lower nibble) + padding (2 bits)
        //   * Byte 13: serviceId bits 0-7 (8 bits)
        //   * Bytes 14-15: sequence counter (16 bits)
        //
        // Worker ID embedding:
        //   - _id0 (8 bits) in rand_a
        //   - _id1-_id4 (32 bits) in bytes 8-11
        //   - _id5 is NOT preserved
        //   Total: 40 bits of worker ID preserved (5 of 6 bytes)
        //
        // ServiceId encoding:
        //   - bits 0-7 (8 bits) in byte 13
        //   - bits 8-11 (4 bits) in rand_a (bytes 6-7)
        //   - bits 12-15 (4 bits) in byte 12
        //   Total: Full 16 bits of serviceId preserved

        // Build the GUID according to RFC 9562 Version 7:
        
        // Bytes 0-3: Most significant 32 bits of timestamp
        var timeLow = (int)(timestamp >> 16);

        // Bytes 4-5: Next 16 bits of timestamp
        var timeMid = (short)(timestamp & 0xFFFF);

        // Bytes 6-7: Version (4 bits) + rand_a (12 bits with 8-bit worker ID from _id0 + 4-bit serviceId bits 8-11)
        // Version 7 = 0111, rand_a = _id0 (8 bits) + serviceId bits 8-11 (4 bits)
        var timeHiAndVersion = (short)(0x7000 | (_id0 << 4) | ((ServiceId >> 8) & 0x0F));

        // Byte 12 (passed as 5th byte parameter): variant (2 bits = 10) + serviceId bits 12-15 (4 bits) + padding (2 bits)
        // Using only lower 4 bits for serviceId bits 12-15, keeping upper 2 bits for variant
        var clkSeqHiAndReserved = (byte)(0x80 | ((ServiceId >> 12) & 0x0F));

        // Byte 13 (passed as 6th byte parameter): serviceId bits 0-7 (8 bits)
        var clkSeqLow = (byte)(ServiceId & 0xFF);

        // Bytes 8-11 (passed as 1st-4th byte parameters): worker ID bytes 1-4 (_id1, _id2, _id3, _id4)
        // Bytes 14-15 (passed as 7th-8th byte parameters): sequence counter (16 bits)
        var extendedSequence = (ushort)(_sequence & 0xFFFF);
        
        return new Guid(
            timeLow,                    // bytes 0-3: timestamp high 32 bits
            timeMid,                    // bytes 4-5: timestamp mid 16 bits
            timeHiAndVersion,           // bytes 6-7: version + rand_a (_id0 + serviceId bits 8-11)
            _id1, _id2, _id3, _id4,     // bytes 8-11: worker ID bytes 1-4
            clkSeqHiAndReserved,        // byte 12: variant + serviceId bits 12-15
            clkSeqLow,                  // byte 13: serviceId bits 0-7
            (byte)(extendedSequence & 0xFF), (byte)(extendedSequence >> 8)  // bytes 14-15: sequence
        );
    }
    /// <summary>
    /// Wait for the next time available for generate Id.
    /// </summary>
    /// <param name="lastTimestamp"></param>
    /// <returns></returns>
    private ulong TillNextMillis(ulong lastTimestamp)
    {
        ulong timestamp = CurrentTime;
        SpinWait.SpinUntil(() => (timestamp = CurrentTime) > lastTimestamp);

        return timestamp;
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion
}
