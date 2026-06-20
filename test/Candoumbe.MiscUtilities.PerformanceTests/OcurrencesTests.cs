// "Copyright (c) Cyrille NDOUMBE.
// Licenced under GNU General Public Licence, version 3.0"

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace Candoumbe.MiscUtilities.PerformanceTests;

[MemoryDiagnoser]
[RPlotExporter]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net90)]
[SimpleJob(RuntimeMoniker.Net10_0)]
public class OcurrencesTests
{

    public string Text { get; set; }

    [Params("lorem", "dolor")]
    public string Search { get; set; }

    [GlobalSetup]
    public async Task ReadFile()
    {
        Text = await File.ReadAllTextAsync("very-long-text.txt");
        Console.WriteLine($"Text of {Text.Length} characters");
    }

    [Benchmark()]
    public int[] Occurrences_extension() => [.. Text.Occurrences(Search)];

    [Benchmark(Baseline = true)]
    public int[] Occurences_with_Linq()
    {
        return [.. Text.Split(" ")
                   .Where(word => word == Search)
                   .Select((_, pos) => pos)];
    }
}