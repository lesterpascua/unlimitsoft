using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using UnlimitSoft.Data.EntityFramework.Utility;

namespace UnlimitSoft.Benchmark.UnlimitSoft.Data.EntityFramework.Utility;


[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net60)]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net90)]
public class StringToArrayConverterBenchmarks
{
    private readonly StringToArrayConverter<int> _converter;

    public StringToArrayConverterBenchmarks()
    {
        _converter = new StringToArrayConverter<int>();
    }

    [Benchmark]
    public int[]? StringToArrayParse()
    {
        var result = (int[]?)_converter.ConvertFromProvider("33,676,784,33,676,784");
        return result;
    }
}
