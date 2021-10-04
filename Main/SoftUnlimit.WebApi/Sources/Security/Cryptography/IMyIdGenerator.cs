﻿using SoftUnlimit.Security.Cryptography;
using System;

namespace SoftUnlimit.WebApi.Sources.Security.Cryptography
{
    public interface IMyIdGenerator : IIdGenerator<Guid>
    {
    }
    public class MyIdGenerator : MicroServiceGenerator, IMyIdGenerator
    {
        public MyIdGenerator(ushort serviceId) : base(serviceId)
        {
        }
    }
}
