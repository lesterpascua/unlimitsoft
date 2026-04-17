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
}