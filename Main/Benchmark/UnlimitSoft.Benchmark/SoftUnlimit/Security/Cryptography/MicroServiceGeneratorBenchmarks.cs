using BenchmarkDotNet.Attributes;
using UnlimitSoft.Security.Cryptography;
using System;

namespace UnlimitSoft.Benchmark.UnlimitSoft.Security.Cryptography
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
