// Copyright (c) 2019-2020 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

using ReactiveMarbles.PropertyChanged.Benchmarks.Moqs;
using UI = ReactiveUI.WhenAnyMixin;
using Old = ReactiveMarbles.PropertyChanged.Benchmarks.Legacy.NotifyPropertyChangedExtensions;
using New = ReactiveMarbles.PropertyChanged.NotifyPropertyChangedExtensions;
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

        [GlobalSetup(Targets = new[] { "SubscribeAndChange_Depth1_UI", "SubscribeAndChange_Depth1_Old", "SubscribeAndChange_Depth1_New", "SubscribeAndChange_Depth1_SourceGen" })]
        public void Depth1Setup()
        {
            _from = new TestClass(1);
        }

        [GlobalSetup(Targets = new[] { "SubscribeAndChange_Depth2_UI", "SubscribeAndChange_Depth2_Old", "SubscribeAndChange_Depth2_New", "SubscribeAndChange_Depth2_SourceGen" })]
        public void Depth2Setup()
        {
            _from = new TestClass(2);
        }

        [GlobalSetup(Targets = new[] { "SubscribeAndChange_Depth3_UI", "SubscribeAndChange_Depth3_Old", "SubscribeAndChange_Depth3_New", "SubscribeAndChange_Depth3_SourceGen" })]
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
        public void SubscribeAndChange_Depth1_UI()
        {
            using var subscription = UI.WhenAnyValue(_from, x => x.Value).Subscribe(x => _to = x);
            PerformMutations(1);
        }

        [BenchmarkCategory("Subscribe and Change Depth 1")]
        [Benchmark]
        public void SubscribeAndChange_Depth1_Old()
        {
            using var subscription = Old.WhenChanged(_from, x => x.Value).Subscribe(x => _to = x);
            PerformMutations(1);
        }

        [BenchmarkCategory("Subscribe and Change Depth 1")]
        [Benchmark]
        public void SubscribeAndChange_Depth1_New()
        {
            using var subscription = New.WhenChanged(_from, x => x.Value).Subscribe(x => _to = x);
            PerformMutations(1);
        }

        [BenchmarkCategory("Subscribe and Change Depth 1")]
        [Benchmark]
        public void SubscribeAndChange_Depth1_SourceGen()
        {
            using var subscription = SourceGen.WhenChanged(_from, x => x.Value).Subscribe(x => _to = x);
            PerformMutations(1);
        }

        [BenchmarkCategory("Subscribe and Change Depth 2")]
        [Benchmark(Baseline = true)]
        public void SubscribeAndChange_Depth2_UI()
        {
            using var subscription = UI.WhenAnyValue(_from, x => x.Child.Value).Subscribe(x => _to = x);
            PerformMutations(2);
        }

        [BenchmarkCategory("Subscribe and Change Depth 2")]
        [Benchmark]
        public void SubscribeAndChange_Depth2_Old()
        {
            using var subscription = Old.WhenChanged(_from, x => x.Child.Value).Subscribe(x => _to = x);
            PerformMutations(2);
        }

        [BenchmarkCategory("Subscribe and Change Depth 2")]
        [Benchmark]
        public void SubscribeAndChange_Depth2_New()
        {
            using var subscription = New.WhenChanged(_from, x => x.Child.Value).Subscribe(x => _to = x);
            PerformMutations(2);
        }

        [BenchmarkCategory("Subscribe and Change Depth 2")]
        [Benchmark]
        public void SubscribeAndChange_Depth2_SourceGen()
        {
            using var subscription = SourceGen.WhenChanged(_from, x => x.Child.Value).Subscribe(x => _to = x);
            PerformMutations(2);
        }

        [BenchmarkCategory("Subscribe and Change Depth 3")]
        [Benchmark(Baseline = true)]
        public void SubscribeAndChange_Depth3_UI()
        {
            using var subscription = UI.WhenAnyValue(_from, x => x.Child.Child.Value).Subscribe(x => _to = x);
            PerformMutations(3);
        }

        [BenchmarkCategory("Subscribe and Change Depth 3")]
        [Benchmark]
        public void SubscribeAndChange_Depth3_Old()
        {
            using var subscription = Old.WhenChanged(_from, x => x.Child.Child.Value).Subscribe(x => _to = x);
            PerformMutations(3);
        }

        [BenchmarkCategory("Subscribe and Change Depth 3")]
        [Benchmark]
        public void SubscribeAndChange_Depth3_New()
        {
            using var subscription = New.WhenChanged(_from, x => x.Child.Child.Value).Subscribe(x => _to = x);
            PerformMutations(3);
        }

        [BenchmarkCategory("Subscribe and Change Depth 3")]
        [Benchmark]
        public void SubscribeAndChange_Depth3_SourceGen()
        {
            using var subscription = SourceGen.WhenChanged(_from, x => x.Child.Child.Value).Subscribe(x => _to = x);
            PerformMutations(3);
        }

        [GlobalSetup(Target = "Change_Depth1_UI")]
        public void Change_Depth1_UISetup()
        {
            Depth1Setup();
            _subscription = UI.WhenAnyValue(_from, x => x.Value).Subscribe(x => _to = x);
        }

        [BenchmarkCategory("Change Depth 1")]
        [Benchmark(Baseline = true)]
        public void Change_Depth1_UI()
        {
            PerformMutations(1);
        }

        [GlobalSetup(Target = "Change_Depth1_Old")]
        public void Change_Depth1_OldSetup()
        {
            Depth1Setup();
            _subscription = Old.WhenChanged(_from, x => x.Value).Subscribe(x => _to = x);
        }

        [BenchmarkCategory("Change Depth 1")]
        [Benchmark]
        public void Change_Depth1_Old()
        {
            PerformMutations(1);
        }

        [GlobalSetup(Target = "Change_Depth1_New")]
        public void Change_Depth1_NewSetup()
        {
            Depth1Setup();
            _subscription = New.WhenChanged(_from, x => x.Value).Subscribe(x => _to = x);
        }

        [BenchmarkCategory("Change Depth 1")]
        [Benchmark]
        public void Change_Depth1_New()
        {
            PerformMutations(1);
        }

        [GlobalSetup(Target = "Change_Depth1_SourceGen")]
        public void Change_Depth1_SourceGenSetup()
        {
            Depth1Setup();
            _subscription = SourceGen.WhenChanged(_from, x => x.Value).Subscribe(x => _to = x);
        }

        [BenchmarkCategory("Change Depth 1")]
        [Benchmark]
        public void Change_Depth1_SourceGen()
        {
            PerformMutations(1);
        }

        [GlobalSetup(Target = "Change_Depth2_UI")]
        public void Change_Depth2_UISetup()
        {
            Depth2Setup();
            _subscription = UI.WhenAnyValue(_from, x => x.Child.Value).Subscribe(x => _to = x);
        }

        [BenchmarkCategory("Change Depth 2")]
        [Benchmark(Baseline = true)]
        public void Change_Depth2_UI()
        {
            PerformMutations(2);
        }

        [GlobalSetup(Target = "Change_Depth2_Old")]
        public void Change_Depth2_OldSetup()
        {
            Depth2Setup();
            _subscription = Old.WhenChanged(_from, x => x.Child.Value).Subscribe(x => _to = x);
        }

        [BenchmarkCategory("Change Depth 2")]
        [Benchmark]
        public void Change_Depth2_Old()
        {
            PerformMutations(2);
        }

        [GlobalSetup(Target = "Change_Depth2_New")]
        public void Change_Depth2_NewSetup()
        {
            Depth2Setup();
            _subscription = New.WhenChanged(_from, x => x.Child.Value).Subscribe(x => _to = x);
        }

        [BenchmarkCategory("Change Depth 2")]
        [Benchmark]
        public void Change_Depth2_New()
        {
            PerformMutations(2);
        }

        [GlobalSetup(Target = "Change_Depth2_SourceGen")]
        public void Change_Depth2_SourceGenSetup()
        {
            Depth2Setup();
            _subscription = SourceGen.WhenChanged(_from, x => x.Child.Value).Subscribe(x => _to = x);
        }

        [BenchmarkCategory("Change Depth 2")]
        [Benchmark]
        public void Change_Depth2_SourceGen()
        {
            PerformMutations(2);
        }

        [GlobalSetup(Target = "Change_Depth3_UI")]
        public void Change_Depth3_UISetup()
        {
            Depth3Setup();
            _subscription = UI.WhenAnyValue(_from, x => x.Child.Child.Value).Subscribe(x => _to = x);
        }

        [BenchmarkCategory("Change Depth 3")]
        [Benchmark(Baseline = true)]
        public void Change_Depth3_UI()
        {
            PerformMutations(3);
        }

        [GlobalSetup(Target = "Change_Depth3_Old")]
        public void Change_Depth3_OldSetup()
        {
            Depth3Setup();
            _subscription = Old.WhenChanged(_from, x => x.Child.Child.Value).Subscribe(x => _to = x);
        }

        [BenchmarkCategory("Change Depth 3")]
        [Benchmark]
        public void Change_Depth3_Old()
        {
            PerformMutations(3);
        }

        [GlobalSetup(Target = "Change_Depth3_New")]
        public void Change_Depth3_NewSetup()
        {
            Depth3Setup();
            _subscription = New.WhenChanged(_from, x => x.Child.Child.Value).Subscribe(x => _to = x);
        }

        [BenchmarkCategory("Change Depth 3")]
        [Benchmark]
        public void Change_Depth3_New()
        {
            PerformMutations(3);
        }

        [GlobalSetup(Target = "Change_Depth3_SourceGen")]
        public void Change_Depth3_SourceGenSetup()
        {
            Depth3Setup();
            _subscription = SourceGen.WhenChanged(_from, x => x.Child.Child.Value).Subscribe(x => _to = x);
        }

        [BenchmarkCategory("Change Depth 3")]
        [Benchmark]
        public void Change_Depth3_SourceGen()
        {
            PerformMutations(3);
        }

        [GlobalCleanup(Targets = new[] { "Change_Depth1_UI", "Change_Depth1_Old", "Change_Depth1_New", "Change_Depth1_SourceGen", "Change_Depth2_UI", "Change_Depth2_Old", "Change_Depth2_New", "Change_Depth2_SourceGen", "Change_Depth3_UI", "Change_Depth3_Old", "Change_Depth3_New", "Change_Depth3_SourceGen" })]
        public void GlobalCleanup()
        {
            _subscription.Dispose();
        }
    }
}
