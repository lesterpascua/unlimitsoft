using SoftUnlimit.CQRS.Event.Json;
using System;

namespace SoftUnlimit.Cloud.Partner.Domain.Handler.Events
{
    public class GenericEventNameResolver : IEventNameResolver
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_"></param>
        /// <returns></returns>
        public Type Resolver(string _) => typeof(CreateGenericCloudEvent);
    }
}
