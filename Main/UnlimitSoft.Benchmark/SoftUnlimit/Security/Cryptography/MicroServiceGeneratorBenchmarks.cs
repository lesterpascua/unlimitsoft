using BenchmarkDotNet.Attributes;
using SoftUnlimit.Security.Cryptography;
using System;

namespace SoftUnlimit.Benchmark.SoftUnlimit.Security.Cryptography
{
    [MemoryDiagnoser]
    public class MicroServiceGeneratorBenchmarks
    {
        private readonly IIdGenerator<Guid> gen = new MicroServiceGenerator(1);

        [Benchmark]
        public Guid TestGeneration()
        {
            return gen.GenerateId();
        }
        [Benchmark]
        public string TestGenerationAsString()
        {
            return gen.GenerateIdAsString();
        }
    }
}
