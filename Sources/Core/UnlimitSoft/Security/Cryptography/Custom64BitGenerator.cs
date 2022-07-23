using UnlimitSoft.Properties;
using System;
using System.Collections.Generic;
using System.Threading;

namespace UnlimitSoft.Security.Cryptography;


/// <summary>
/// 
/// </summary>
public class Custom64BitGenerator : IIdGenerator<ulong>
{
    private const ulong Mask = 18446744073709551615L;       // -1L

    private readonly object _sync = new();                  // Object used as a monitor for threads synchronization.
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
        _maxSequence = Mask ^ (Mask << settings.SequenceBits);
        ulong MaxIdentifierBits = Mask ^ (Mask << settings.IdentifierBits);

        if (sequence > _maxSequence || sequence < 0)
            throw new InvalidOperationException(string.Format(Resources.InvalidOperationException_PropertyBetween, "Sequence", _maxSequence, 0));
        if (identifier > MaxIdentifierBits || identifier < 0)
            throw new InvalidOperationException(string.Format(Resources.InvalidOperationException_PropertyBetween, "Identifier", MaxIdentifierBits, 0));

        _sequence = sequence;
        Identifier = identifier;

        _startEpoch = settings.StartEpoch;
        _sequenceBits = settings.SequenceBits;
        _identifierBits = settings.IdentifierBits;

        _timestampLeftShift = _identifierBits + _sequenceBits;

        Id = identifier.ToString();
    }

    /// <summary>
    /// 
    /// </summary>
    public string Id { get; }
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
        lock(_sync)
            return NextId();
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
        get { return (ulong)(DateTime.UtcNow - _startEpoch).TotalMilliseconds; }
    }

    #endregion

    #region Private Methods

    private ulong NextId()
    {
        var timestamp = CurrentTime;
        if (timestamp < _lastTimestamp)
            throw new InvalidOperationException(string.Format(Resources.InvalidOperationException_ClockMovedBackwards, (_lastTimestamp - timestamp)));

        if (_lastTimestamp == timestamp)
        {
            if (++_sequence > _maxSequence)
                timestamp = TillNextMillis(_lastTimestamp);
        } else
            _sequence = 0;

        _lastTimestamp = timestamp;
        return (timestamp << _timestampLeftShift) | (Identifier << _identifierBits) | _sequence;
    }
    private ulong TillNextMillis(ulong lastTimestamp)
    {
        var timestamp = CurrentTime;
        SpinWait.SpinUntil(() => (timestamp = CurrentTime) > lastTimestamp);

        return timestamp;
    }

    #endregion
}
