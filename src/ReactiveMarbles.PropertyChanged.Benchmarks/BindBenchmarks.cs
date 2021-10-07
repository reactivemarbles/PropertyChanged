// Copyright (c) 2019-2020 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

using ReactiveMarbles.PropertyChanged.Benchmarks.Moqs;

using New = ReactiveMarbles.PropertyChanged.BindExtensions;
using Old = ReactiveMarbles.PropertyChanged.Benchmarks.Legacy.BindExtensions;
using UI = ReactiveUI.PropertyBindingMixins;

namespace ReactiveMarbles.PropertyChanged.Benchmarks
{
    /// <summary>
    /// Benchmarks for binding.
    /// </summary>
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [MemoryDiagnoser]
    [MarkdownExporterAttribute.GitHub]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    public class BindBenchmarks
    {
        private TestClass _from;
        private TestClass _to;
        private IDisposable _binding;

        /// <summary>
        /// The number mutations to perform.
        /// </summary>
        [Params(1, 10, 100, 1000)]
        public int Changes;

        [GlobalSetup(Targets = new[] { "BindAndChange_Depth1_UI", "BindAndChange_Depth1_Old", "BindAndChange_Depth1_New" })]
        public void Depth1Setup()
        {
            _from = new TestClass(1);
            _to = new TestClass(1);
        }

        [GlobalSetup(Targets = new[] { "BindAndChange_Depth2_UI", "BindAndChange_Depth2_Old", "BindAndChange_Depth2_New" })]
        public void Depth2Setup()
        {
            _from = new TestClass(2);
            _to = new TestClass(2);
        }

        [GlobalSetup(Targets = new[] { "BindAndChange_Depth3_UI", "BindAndChange_Depth3_Old", "BindAndChange_Depth3_New" })]
        public void Depth3Setup()
        {
            _from = new TestClass(3);
            _to = new TestClass(3);
        }

        public void PerformMutations(int depth)
        {
            // We loop through the changes, alternating mutations to the source and destination at every depth.
            int d2 = depth * 2;
            for (int i = 0; i < Changes; ++i)
            {
                int a = i % d2;
                TestClass t = (a % 2) > 0 ? _to : _from;
                t.Mutate(a / 2);
            }
        }

        [BenchmarkCategory("Bind and Change Depth 1")]
        [Benchmark(Baseline = true)]
        public void BindAndChange_Depth1_UI()
        {
            using ReactiveUI.IReactiveBinding<TestClass, (object view, bool isViewModel)> binding = UI.Bind(_from, _to, x => x.Value, x => x.Value);
            PerformMutations(1);
        }

        [BenchmarkCategory("Bind and Change Depth 1")]
        [Benchmark]
        public void BindAndChange_Depth1_Old()
        {
            using IDisposable binding = Old.Bind(_from, _to, x => x.Value, x => x.Value);
            PerformMutations(1);
        }

        [BenchmarkCategory("Bind and Change Depth 1")]
        [Benchmark]
        public void BindAndChange_Depth1_New()
        {
            using IDisposable binding = New.BindTwoWay(_from, _to, x => x.Value, x => x.Value);
            PerformMutations(1);
        }

        [BenchmarkCategory("Bind and Change Depth 2")]
        [Benchmark(Baseline = true)]
        public void BindAndChange_Depth2_UI()
        {
            using ReactiveUI.IReactiveBinding<TestClass, (object view, bool isViewModel)> binding = UI.Bind(_from, _to, x => x.Child.Value, x => x.Child.Value);
            PerformMutations(2);
        }

        [BenchmarkCategory("Bind and Change Depth 2")]
        [Benchmark]
        public void BindAndChange_Depth2_Old()
        {
            using IDisposable binding = Old.Bind(_from, _to, x => x.Child.Value, x => x.Child.Value);
            PerformMutations(2);
        }

        [BenchmarkCategory("Bind and Change Depth 2")]
        [Benchmark]
        public void BindAndChange_Depth2_New()
        {
            using IDisposable binding = New.BindTwoWay(_from, _to, x => x.Child.Value, x => x.Child.Value);
            PerformMutations(2);
        }

        [BenchmarkCategory("Bind and Change Depth 3")]
        [Benchmark(Baseline = true)]
        public void BindAndChange_Depth3_UI()
        {
            using ReactiveUI.IReactiveBinding<TestClass, (object view, bool isViewModel)> binding = UI.Bind(_from, _to, x => x.Child.Child.Value, x => x.Child.Child.Value);
            PerformMutations(3);
        }

        [BenchmarkCategory("Bind and Change Depth 3")]
        [Benchmark]
        public void BindAndChange_Depth3_Old()
        {
            using IDisposable binding = Old.Bind(_from, _to, x => x.Child.Child.Value, x => x.Child.Child.Value);
            PerformMutations(3);
        }

        [BenchmarkCategory("Bind and Change Depth 3")]
        [Benchmark]
        public void BindAndChange_Depth3_New()
        {
            using IDisposable binding = New.BindTwoWay(_from, _to, x => x.Child.Child.Value, x => x.Child.Child.Value);
            PerformMutations(3);
        }

        [GlobalSetup(Target = "Change_Depth1_UI")]
        public void Change_Depth1_UISetup()
        {
            Depth1Setup();
            _binding = UI.Bind(_from, _to, x => x.Value, x => x.Value);
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
            _binding = Old.Bind(_from, _to, x => x.Value, x => x.Value);
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
            _binding = New.BindTwoWay(_from, _to, x => x.Value, x => x.Value);
        }

        [BenchmarkCategory("Change Depth 1")]
        [Benchmark]
        public void Change_Depth1_New()
        {
            PerformMutations(1);
        }

        [GlobalSetup(Target = "Change_Depth2_UI")]
        public void Change_Depth2_UISetup()
        {
            Depth2Setup();
            _binding = UI.Bind(_from, _to, x => x.Child.Value, x => x.Child.Value);
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
            _binding = Old.Bind(_from, _to, x => x.Child.Value, x => x.Child.Value);
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
            _binding = New.BindTwoWay(_from, _to, x => x.Child.Value, x => x.Child.Value);
        }

        [BenchmarkCategory("Change Depth 2")]
        [Benchmark]
        public void Change_Depth2_New()
        {
            PerformMutations(2);
        }

        [GlobalSetup(Target = "Change_Depth3_UI")]
        public void Change_Depth3_UISetup()
        {
            Depth3Setup();
            _binding = UI.Bind(_from, _to, x => x.Child.Child.Value, x => x.Child.Child.Value);
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
            _binding = Old.Bind(_from, _to, x => x.Child.Child.Value, x => x.Child.Child.Value);
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
            _binding = New.BindTwoWay(_from, _to, x => x.Child.Child.Value, x => x.Child.Child.Value);
        }

        [BenchmarkCategory("Change Depth 3")]
        [Benchmark]
        public void Change_Depth3_New()
        {
            PerformMutations(3);
        }

        [GlobalCleanup(Targets = new[] { "Change_Depth1_UI", "Change_Depth1_Old", "Change_Depth1_New", "Change_Depth2_UI", "Change_Depth2_Old", "Change_Depth2_New", "Change_Depth3_UI", "Change_Depth3_Old", "Change_Depth3_New" })]
        public void GlobalCleanup()
        {
            _binding.Dispose();
        }
    }
}