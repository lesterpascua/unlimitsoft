using SoftUnlimit.Security;
using SoftUnlimit.Security.Cryptography;
using System;

namespace SoftUnlimit.Cloud.Security.Cryptography
{
    public interface ICloudIdGenerator : IIdGenerator<Guid>, IServiceMetadata
    {
    }
}
