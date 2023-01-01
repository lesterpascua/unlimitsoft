using UnlimitSoft.Security;
using UnlimitSoft.Security.Cryptography;

namespace UnlimitSoft.WebApi.EventSourced.CQRS;


public interface IMyIdGenerator : IIdGenerator<Guid>, IServiceMetadata
{
}
public sealed class MyIdGenerator : MicroServiceGenerator, IMyIdGenerator
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="serviceId"></param>
    public MyIdGenerator(ushort serviceId) :
        base(serviceId)
    {
    }
}
