// .Net Utils - Misc utility classes/functions for use in .Net libraries.
// Written in 2023 by Eric Orth
//
// To the extent possible under law, the author(s) have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see http://creativecommons.org/publicdomain/zero/1.0/.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Collections.Immutable;

namespace DotNetUtils.Benchmarks
{
    public class ImmutableListSliceListBenchmarks : ImmutableListSliceBenchmarks
    {
        [Benchmark]
        public IImmutableList<int> ToImmutableList()
        {
            return List.ToImmutableList((ListSize / 2)..(ListSize / 2 + SliceSize));
        }

        [Benchmark]
        public IImmutableList<int> RawAccess()
        {
            ImmutableList<int>.Builder builder = ImmutableList.CreateBuilder<int>();
            for (int i = ListSize / 2; i < ListSize / 2 + SliceSize; i++)
            {
                builder.Add(List[i]);
            }
            return builder.ToImmutable();
        }

        [Benchmark(Baseline = true)]
        public IImmutableList<int> Take()
        {
            return List.Take((ListSize / 2)..(ListSize / 2 + SliceSize)).ToImmutableList();
        }
    }

    public class ImmutableListSliceSpanBenchmarks : ImmutableListSliceBenchmarks
    {
        [Benchmark]
        public ReadOnlySpan<int> ToImmutableSpan()
        {
            return List.ToImmutableSpan((ListSize / 2)..(ListSize / 2 + SliceSize));
        }

        [Benchmark]
        public ReadOnlySpan<int> Subspan()
        {
            return List.ToArray().AsSpan()[(ListSize / 2)..(ListSize / 2 + SliceSize)];
        }

        [Benchmark(Baseline = true)]
        public ReadOnlySpan<int> Take()
        {
            return List.Take((ListSize / 2)..(ListSize / 2 + SliceSize)).ToImmutableArray().AsSpan();
        }
    }

    public class ImmutableListSliceBenchmarks
    {
        public static void RunBenchmarks()
        {
            BenchmarkRunner.Run<ImmutableListSliceListBenchmarks>();
            BenchmarkRunner.Run<ImmutableListSliceSpanBenchmarks>();
        }

        public enum ListType
        {
            ImmutableList,
            ImmutableArray,
            List,
        }

        [Params(10, 100, 1000, 5000)]
        public int SliceSize { get; set; }

        [Params(10000, 1000000, 100000000)]
        public int ListSize { get; set; }

        [Params(ListType.ImmutableList, ListType.ImmutableArray, ListType.List)]
        public ListType Type { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            IEnumerable<int> enumerable = Enumerable.Range(0, ListSize);
            switch (Type)
            {
                case ListType.ImmutableList:
                    _list = enumerable.ToImmutableList();
                    break;
                case ListType.ImmutableArray:
                    _list = enumerable.ToImmutableArray();
                    break;
                case ListType.List:
                    _list = enumerable.ToList();
                    break;

            }
        }

        protected IList<int> List => _list;

        private IList<int> _list = new List<int>();
    }
}
