using UnlimitSoft.Security;
using UnlimitSoft.Security.Cryptography;

namespace OIU.Libs.Security.Cryptography;


/// <summary>
/// Interface to create unique identifier across multiple services.
/// </summary>
public interface IUniqueGenerator : IIdGenerator<Guid>, IServiceMetadata
{
}
/// <summary>
/// Default implementation of <see cref="IUniqueGenerator"/>
/// </summary>
public sealed class UniqueGenerator : MicroServiceGenerator, IUniqueGenerator
{
    private static IUniqueGenerator? _default;


    /// <summary>
    /// 
    /// </summary>
    /// <param name="serviceId"></param>
    public UniqueGenerator(int serviceId)
        : base((ushort)serviceId)
    {
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="serviceId"></param>
    /// <param name="workerId">Worker identifier mac addresss coding in base64. the identifier is array of 6 entries.</param>
    public UniqueGenerator(int serviceId, byte[] workerId)
        : base((ushort)serviceId, workerId)
    {
    }

    /// <summary>
    /// Get default generator
    /// </summary>
    public static IUniqueGenerator Default
    {
        get => _default ?? throw new InvalidOperationException("Value is not assigned yet");
        set => Interlocked.CompareExchange(ref _default, value, null);
    }

    /// <summary>
    /// Get the default generator, creating and assigning it with <paramref name="factory"/> when it is not assigned yet.
    /// </summary>
    /// <remarks>
    /// Used by the host registration so a process has exactly one generator: the one injected is always the same instance as
    /// <see cref="Default"/>. Two generators with the same service and worker identifier keep their own timestamp and sequence, so they can
    /// generate the same identifier in the same millisecond.
    /// </remarks>
    /// <param name="factory">Create the generator when the default is not assigned yet.</param>
    /// <returns>The default generator.</returns>
    public static IUniqueGenerator GetOrSetDefault(Func<IUniqueGenerator> factory)
    {
        var current = Volatile.Read(ref _default);
        if (current is not null)
            return current;

        Interlocked.CompareExchange(ref _default, factory(), null);
        return _default!;
    }
}