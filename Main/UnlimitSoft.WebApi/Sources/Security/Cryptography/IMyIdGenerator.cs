using UnlimitSoft.Security;
using UnlimitSoft.Security.Cryptography;
using System;

namespace UnlimitSoft.WebApi.Sources.Security.Cryptography
{
    public interface IMyIdGenerator : IIdGenerator<Guid>, IServiceMetadata
    {
    }
    public class MyIdGenerator : MicroServiceGenerator, IMyIdGenerator
    {
        public MyIdGenerator(ushort serviceId) : base(serviceId)
        {
        }
    }
}
