using UnlimitSoft.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;

namespace UnlimitSoft.Security.Cryptography;


/// <summary>
/// A decentralized, k-ordered id generator. Generated ids are Guid (128-bit wide). Allow create unique identifier for 
/// entities across microservices
/// </summary>
/// <remarks>
/// <list>
///     <item>48-bit timestamp - milliseconds since the epoch (Jan 1 1970)</item>
///     <item>16-bit service</item>
///     <item>48-bit worker id</item>
///     <item>16-bit secuence</item>
/// </list>
/// </remarks>
public class MicroServiceGenerator : IIdGenerator<Guid>, IServiceMetadata
{
    private readonly ulong _startEpoch;

#if NET9_0_OR_GREATER
    private readonly Lock _sync = new();                    // Object used as a monitor for threads synchronization.
#else
    private readonly object _sync = new();                  // Object used as a monitor for threads synchronization.
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

        if (workerId == null)
            throw new ArgumentNullException(nameof(workerId));
        if (workerId.Length != 6)
            throw new ArgumentException("Parameter must be an array of length equal 6.", nameof(workerId));


        _id0 = workerId[0]; _id1 = workerId[1];
        _id2 = workerId[2]; _id3 = workerId[3];
        _id4 = workerId[4]; _id5 = workerId[5];
        _startEpoch = (ulong)epoch.Value.Ticks / TimeSpan.TicksPerMillisecond;

        ServiceId = serviceId;
        WorkerId = Convert.ToBase64String(workerId);
        Id = Convert.ToBase64String(BitConverter.GetBytes(serviceId).Concat(workerId).ToArray());
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

    #region Private Methods

    /// <summary>
    /// Current amount of miliseconds from start epoch
    /// </summary>
    private ulong CurrentTime => ((ulong)SysClock.GetUtcNow().Ticks / TimeSpan.TicksPerMillisecond) - _startEpoch;
    /// <summary>
    /// Get next identifier.
    /// </summary>
    /// <returns></returns>
    private Guid NextId()
    {
        var timestamp = CurrentTime;
        if (timestamp < _lastTimestamp)
            throw new InvalidOperationException(string.Format(Resources.InvalidOperationException_ClockMovedBackwards, (_lastTimestamp - timestamp)));

        if (_lastTimestamp == timestamp)
        {
            if (++_sequence > ushort.MaxValue)
                timestamp = TillNextMillis(_lastTimestamp);
        }
        else
            _sequence = 0;

        _lastTimestamp = timestamp;
        return new Guid(
            (int)(_lastTimestamp >> 16 & 0xFFFFFFFF), (short)(_lastTimestamp & 0xFFFF),
            (short)ServiceId,
            _id5, _id4, _id3, _id2, _id1, _id0,
            (byte)(_sequence >> 8 & 0xFF), (byte)(_sequence >> 0 & 0xFF));
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
