// Licensed under the Apache License, Version 2.0 (the "License").
// See the LICENSE file in the project root for more information.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using ReactiveMarbles.PropertyChanged.Benchmarks.Moqs;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using New = ReactiveMarbles.PropertyChanged.BindExtensions;
using Old = ReactiveMarbles.PropertyChanged.Benchmarks.Legacy.BindExtensions;
using UI = ReactiveUI.PropertyBindingMixins;

namespace ReactiveMarbles.PropertyChanged.Benchmarks
{
    /// <summary>
    /// Benchmarks for the property changed.
    /// </summary>
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [MemoryDiagnoser]
    [MarkdownExporterAttribute.GitHub]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    public class BindBenchmarks
    {

        /// <summary>
        /// The parameters for the tests.
        /// </summary>
        [SuppressMessage("Design", "SA1401: field should be private", Justification = "needed by benchmark")]
        [SuppressMessage("Design", "CA1051: field should be private", Justification = "needed by benchmark")]
        [Params(1, 2, 3)]
        public int Depth;

        [SuppressMessage("Design", "SA1401: field should be private", Justification = "needed by benchmark")]
        [SuppressMessage("Design", "CA1051: field should be private", Justification = "needed by benchmark")]
        [Params( 1, 10, 100, 1000)]
        public int Changes;
        
        private TestClass _from;
        private TestClass _to;
        private IDisposable? _binding;
        private Expression<Func<TestClass, int>> _propertyExpression;

        [GlobalSetup(Targets = new[] { nameof(BindAndChangeUI), nameof(BindAndChangeOld), nameof(BindAndChangeNew), nameof(NoBind) })]
        public void CommonSetup()
        {
            _from = new TestClass(Depth);
            _to = new TestClass(Depth);
            _propertyExpression = TestClass.GetValuePropertyExpression(Depth);
        }

        //[BenchmarkCategory("No Binding")]
        //[Benchmark(Baseline = true)]
        public void NoBind()
        {
            // We loop through the changes, alternating mutations to the source and destination
            // at every depth.
            var d2 = Depth * 2;
            for (var i = 0; i < Changes; ++i)
            {
                var a = i % d2;
                var t = (a % 2) > 0 ? _to : _from;
                t.Mutate(a / 2);
            }
        }

        [BenchmarkCategory("Bind and Change")]
        [Benchmark(Baseline = true)]
        public void BindAndChangeUI()
        {
            using var binding = UI.Bind(_from, _to, _propertyExpression, _propertyExpression);

            NoBind();
        }

        [BenchmarkCategory("Bind and Change")]
        [Benchmark]
        public void BindAndChangeOld()
        {
            using var binding = Old.Bind(_from, _to, _propertyExpression, _propertyExpression);
            
            NoBind();
        }

        [BenchmarkCategory("Bind and Change")]
        [Benchmark]
        public void BindAndChangeNew()
        {
            using var binding = New.Bind(_from, _to, _propertyExpression, _propertyExpression);

            NoBind();
        }

        [GlobalSetup(Target = nameof(ChangeUI))]
        public void ChangUISetup()
        {
            CommonSetup();
            _binding = UI.Bind(_from, _to, _propertyExpression, _propertyExpression);
        }

        [BenchmarkCategory("Change")]
        [Benchmark(Baseline = true)]
        public void ChangeUI()
        {
            NoBind();
        }

        [GlobalSetup(Target = nameof(ChangeOld))]
        public void ChangOldSetup()
        {
            CommonSetup();
            _binding = Old.Bind(_from, _to, _propertyExpression, _propertyExpression);
        }

        [BenchmarkCategory("Change")]
        [Benchmark]
        public void ChangeOld()
        {
            NoBind();
        }

        [GlobalSetup(Target = nameof(ChangeNew))]
        public void ChangNewSetup()
        {
            CommonSetup();
            _binding = New.Bind(_from, _to, _propertyExpression, _propertyExpression);
        }

        [BenchmarkCategory("Change")]
        [Benchmark]
        public void ChangeNew()
        {
            NoBind();
        }

        [GlobalCleanup(Targets = new[] { nameof(ChangeUI), nameof(ChangeOld), nameof(ChangeNew) })]
        public void GlobalCleanup()
        {
            // Disposing logic
            _binding!.Dispose();
        }
    }
}
