using BenchmarkDotNet.Attributes;
using System.Threading.Tasks;
using UnlimitSoft.Benchmark.SoftUnlimit.CQRS.Labs;

namespace SoftUnlimit.Benchmark.SoftUnlimit.CQRS
{
    [MemoryDiagnoser]
    public class CommandBenchmark
    {
        private readonly MediatRLab _mediatR;
        private readonly CommandDispatcherLab _unlimitSoft;

        public CommandBenchmark()
        {
            _mediatR = new MediatRLab();
            _unlimitSoft = new CommandDispatcherLab(false);
        }

        [Benchmark]
        public async Task MediatR()
        {
            await _mediatR.DispatchCommand();
        }
        [Benchmark]
        public async Task UnlimitSoft()
        {
            await _unlimitSoft.Dispatch();
        }
    }
}
