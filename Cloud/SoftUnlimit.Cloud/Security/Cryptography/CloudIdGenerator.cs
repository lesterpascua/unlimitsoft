using SoftUnlimit.Security.Cryptography;

namespace SoftUnlimit.Cloud.Security.Cryptography
{
    /// <summary>
    /// 
    /// </summary>
    public class CloudIdGenerator : MicroServiceGenerator, ICloudIdGenerator
    {
        public CloudIdGenerator(ushort serviceId) :
            base(serviceId)
        {
        }
    }
}
