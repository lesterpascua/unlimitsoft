using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Threading;
using UnlimitSoft.Properties;

namespace UnlimitSoft.Security.Cryptography;


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
    /// <param name="identifier">Identifier can be a mac addresss. the identifier is array of 6 entries.</param>
    /// <param name="epoch">Epoc start time.</param>
    public IdGuidGenerator(byte[] identifier, DateTime epoch)
    {
        if (identifier == null)
            throw new ArgumentNullException(nameof(identifier));
        if (identifier.Length != 6)
            throw new ArgumentException("Parameter must be an array of length equal 6.", nameof(identifier));

        _id0 = identifier[0]; _id1 = identifier[1];
        _id2 = identifier[2]; _id3 = identifier[3];
        _id4 = identifier[4]; _id5 = identifier[5];
        _startEpoch = (ulong)epoch.Ticks / TimeSpan.TicksPerMillisecond;

        Id = Convert.ToBase64String(identifier);
        WorkerId = (ushort)(_id0 << (8 * 0) | _id1 << (8 * 1));
        ServiceId = (uint)(_id2 << (8 * 0) | _id3 << (8 * 1) | _id4 << (8 * 2) | _id5 << (8 * 3));
    }


    /// <summary>
    /// Generator identifier as string.
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

    #endregion Public Methods

    #region Private Methods

    private ulong CurrentTime => ((ulong)SysClock.GetUtcNow().Ticks / TimeSpan.TicksPerMillisecond) - _startEpoch;

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
        ulong timestamp = CurrentTime;
        SpinWait.SpinUntil(() => (timestamp = CurrentTime) > lastTimestamp);

        return timestamp;
    }

    /// <summary>
    /// Create identifier from service and worker
    /// </summary>
    /// <param name="service"></param>
    /// <param name="worker"></param>
    /// <returns></returns>
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
}
