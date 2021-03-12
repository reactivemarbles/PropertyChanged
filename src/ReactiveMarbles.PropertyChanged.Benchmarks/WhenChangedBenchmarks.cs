// Copyright (c) 2019-2020 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

using ReactiveMarbles.PropertyChanged.Benchmarks.Moqs;
using SourceGen = NotifyPropertyChangedExtensions;

namespace ReactiveMarbles.PropertyChanged.Benchmarks
{
    /// <summary>
    /// Benchmarks for the property changed.
    /// </summary>
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [MemoryDiagnoser]
    [MarkdownExporterAttribute.GitHub]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    public class WhenChangedBenchmarks
    {
        private TestClass _from;
        private int _to;
        private IDisposable _subscription;

        /// <summary>
        /// The number mutations to perform.
        /// </summary>
        [Params(1, 10, 100, 1000)]
        public int Changes;

        [GlobalSetup(Targets = new[] { "SubscribeAndChange_Depth1_SourceGen" })]
        public void Depth1Setup()
        {
            _from = new TestClass(1);
        }

        [GlobalSetup(Targets = new[] { "SubscribeAndChange_Depth2_SourceGen" })]
        public void Depth2Setup()
        {
            _from = new TestClass(2);
        }

        [GlobalSetup(Targets = new[] { "SubscribeAndChange_Depth3_SourceGen" })]
        public void Depth3Setup()
        {
            _from = new TestClass(3);
        }

        public void PerformMutations(int depth)
        {
            // We loop through the changes, creating mutations at every depth.
            for (var i = 0; i < Changes; ++i)
            {
                _from.Mutate(i % depth);
            }
        }

        [BenchmarkCategory("Subscribe and Change Depth 1")]
        [Benchmark(Baseline = true)]
        public void SubscribeAndChange_Depth1_SourceGen()
        {
            using var subscription = SourceGen.WhenChanged(_from, x => x.Value).Subscribe(x => _to = x);
            PerformMutations(1);
        }

        [BenchmarkCategory("Subscribe and Change Depth 2")]
        [Benchmark(Baseline = true)]
        public void SubscribeAndChange_Depth2_SourceGen()
        {
            using var subscription = SourceGen.WhenChanged(_from, x => x.Child.Value).Subscribe(x => _to = x);
            PerformMutations(2);
        }

        [BenchmarkCategory("Subscribe and Change Depth 3")]
        [Benchmark(Baseline = true)]
        public void SubscribeAndChange_Depth3_SourceGen()
        {
            using var subscription = SourceGen.WhenChanged(_from, x => x.Child.Child.Value).Subscribe(x => _to = x);
            PerformMutations(3);
        }

        [GlobalSetup(Target = "Change_Depth1_SourceGen")]
        public void Change_Depth1_SourceGenSetup()
        {
            Depth1Setup();
            _subscription = SourceGen.WhenChanged(_from, x => x.Value).Subscribe(x => _to = x);
        }

        [BenchmarkCategory("Change Depth 1")]
        [Benchmark(Baseline = true)]
        public void Change_Depth1_SourceGen()
        {
            PerformMutations(1);
        }

        [GlobalSetup(Target = "Change_Depth2_SourceGen")]
        public void Change_Depth2_SourceGenSetup()
        {
            Depth2Setup();
            _subscription = SourceGen.WhenChanged(_from, x => x.Child.Value).Subscribe(x => _to = x);
        }

        [BenchmarkCategory("Change Depth 2")]
        [Benchmark(Baseline = true)]
        public void Change_Depth2_SourceGen()
        {
            PerformMutations(2);
        }

        [GlobalSetup(Target = "Change_Depth3_SourceGen")]
        public void Change_Depth3_SourceGenSetup()
        {
            Depth3Setup();
            _subscription = SourceGen.WhenChanged(_from, x => x.Child.Child.Value).Subscribe(x => _to = x);
        }

        [BenchmarkCategory("Change Depth 3")]
        [Benchmark(Baseline = true)]
        public void Change_Depth3_SourceGen()
        {
            PerformMutations(3);
        }

        [GlobalCleanup(Targets = new[] { "Change_Depth1_SourceGen", "Change_Depth2_SourceGen", "Change_Depth3_SourceGen" })]
        public void GlobalCleanup()
        {
            _subscription.Dispose();
        }
    }
}
