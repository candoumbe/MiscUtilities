// "Copyright (c) Cyrille NDOUMBE.
// Licenced under GNU General Public Licence, version 3.0"

using BenchmarkDotNet.Attributes;

namespace Candoumbe.MiscUtilities.PerformanceTests.Extensions
{
    internal class ToArrayTests
    {

        [Params(1, 10, 1_000, 10_000)]
        public int Count { get; set; }

        private readonly Array _array;
        private readonly int[] _genericArray;

        private readonly IList<int> _numbers;

        public ToArrayTests()
        {
            _numbers = Enumerable.Range(0, Count)
                      .ToArray();
            _array = Array.CreateInstance(typeof(int), Count);
            _genericArray = new int[Count];
        }

        [Benchmark(Baseline = true)]
        public int[] ToArrayExtension() => _array.ToArray<int>();

        [Benchmark]
        public int[] ToArray_Using_ref()
        {
            for (int i = 0; i < Count; i++)
            {
                _genericArray[i] = _numbers[i];
            }

            return _genericArray.ToArray();
        }
    }
}
