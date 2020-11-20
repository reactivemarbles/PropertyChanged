// Copyright (c) 2019-2020 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

using ReactiveMarbles.PropertyChanged.Benchmarks.Moqs;
using ReactiveUI;
using New = ReactiveMarbles.PropertyChanged.NotifyPropertyChangedExtensions;
using Old = ReactiveMarbles.PropertyChanged.Benchmarks.Legacy.NotifyPropertyChangedExtensions;
using UI = ReactiveUI.WhenAnyMixin;

namespace ReactiveMarbles.PropertyChanged.Benchmarks
{
    /// <summary>
    /// Benchmarks for the property changed.
    /// </summary>
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [MemoryDiagnoser]
    [MarkdownExporterAttribute.GitHub]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    public class PropertyChangesBenchmarks
    {
        /// <summary>
        /// The parameters for the tests.
        /// </summary>
        [SuppressMessage("Design", "SA1401: field should be private", Justification = "needed by benchmark")]
        [SuppressMessage("Design", "CA1051: field should be private", Justification = "needed by benchmark")]
        [Params(1, 2, 3)]
        public int Depth;

        /// <summary>
        /// The parameters for the tests.
        /// </summary>
        [SuppressMessage("Design", "SA1401: field should be private", Justification = "needed by benchmark")]
        [SuppressMessage("Design", "CA1051: field should be private", Justification = "needed by benchmark")]
        [Params(1, 10, 100, 1000)]
        public int Changes;

        private TestClass _from;
        private int _to;
        private IDisposable? _subscription;

        private Expression<Func<TestClass, int>> _propertyExpression;

        [GlobalSetup(Targets = new[] { nameof(SubscribeAndChangeUI), nameof(SubscribeAndChangeOld), nameof(SubscribeAndChangeNew), nameof(NoBind) })]
        public void CommonSetup()
        {
            _from = new TestClass(Depth);
            _propertyExpression = TestClass.GetValuePropertyExpression(Depth);
        }

        //[BenchmarkCategory("No Binding")]
        //[Benchmark(Baseline = true)]
        public void NoBind()
        {
            // We loop through the changes, creating mutations at every depth.
            for (var i = 0; i < Changes; ++i)
            {
                _from.Mutate(i % Depth);
            }
        }

        [BenchmarkCategory("Subscribe and Change")]
        [Benchmark(Baseline = true)]
        public void SubscribeAndChangeUI()
        {
            using var subscription = UI.WhenAnyValue(_from, _propertyExpression).Subscribe(x => _to = x);

            NoBind();
        }

        [BenchmarkCategory("Subscribe and Change")]
        [Benchmark]
        public void SubscribeAndChangeOld()
        {
            using var subscription = Old.WhenPropertyValueChanges(_from, _propertyExpression).Subscribe(x => _to = x);

            NoBind();
        }

        [BenchmarkCategory("Subscribe and Change")]
        [Benchmark]
        public void SubscribeAndChangeNew()
        {
            using var subscription = New.WhenPropertyValueChanges(_from, _propertyExpression).Subscribe(x => _to = x);

            NoBind();
        }

        [GlobalSetup(Target = nameof(ChangeUI))]
        public void ChangUISetup()
        {
            CommonSetup();
            _subscription = UI.WhenAnyValue(_from, _propertyExpression).Subscribe(x => _to = x);
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
            _subscription = Old.WhenPropertyValueChanges(_from, _propertyExpression).Subscribe(x => _to = x);
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
            _subscription = New.WhenPropertyValueChanges(_from, _propertyExpression).Subscribe(x => _to = x);
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
            _subscription!.Dispose();
        }
    }
}