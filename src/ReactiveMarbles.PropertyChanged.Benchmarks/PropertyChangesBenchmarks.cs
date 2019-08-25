// Copyright (c) 2019 Glenn Watson. All rights reserved.
// Glenn Watson licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using ReactiveMarbles.PropertyChanged.Benchmarks.Moqs;
using ReactiveUI;

namespace ReactiveMarbles.PropertyChanged.Benchmarks
{
    /// <summary>
    /// Benchmarks for the property changed.
    /// </summary>
    [CoreJob]
    [MemoryDiagnoser]
    [MarkdownExporterAttribute.GitHub]
    public class PropertyChangesBenchmarks
    {
        /// <summary>
        /// The parameters for the tests.
        /// </summary>
        [SuppressMessage("Design", "SA1401: field should be private", Justification = "needed by benchmark")]
        [SuppressMessage("Design", "CA1051: field should be private", Justification = "needed by benchmark")]
        [Params(1, 10, 100, 1000)]
        public int N;

        private Random _random;

        /// <summary>
        /// Setup for each benchmark method.
        /// </summary>
        [GlobalSetup]
        public void SetupRandom()
        {
            _random = new Random(1);
        }

        /// <summary>
        /// Tests the reactive marbles tests.
        /// </summary>
        [Benchmark]
        public void ReactiveMarblesChanges()
        {
            var a = new A();
            var b = new B();
            a.B = b;
            var c = new C();
            b.C = c;

            string testValue = "ignore";
            a.WhenPropertyChanges(x => x.B.C.Test).Subscribe(x => testValue = x);

            for (int i = 0; i < N; ++i)
            {
                GenerateRandom(a, ref b, ref c);
            }
        }

        /// <summary>
        /// Tests the reactive ui tests.
        /// </summary>
        [Benchmark]
        public void ReactiveUIChanges()
        {
            var a = new A();
            var b = new B();
            a.B = b;
            var c = new C();
            b.C = c;

            string testValue = "ignore";

            a.WhenAnyValue(x => x.B.C.Test).Subscribe(x => testValue = x);

            for (int i = 0; i < N; ++i)
            {
                GenerateRandom(a, ref b, ref c);
            }
        }

        private void GenerateRandom(A a, ref B b, ref C c)
        {
            var choice = _random.Next(3);

            switch (choice)
            {
                case 0:
                    c = new C();
                    b = new B() { C = c };
                    a.B = b;
                    break;
                case 1:
                    c = new C();
                    b.C = c;
                    break;
                case 2:
                {
                    string testString = "test string" + _random.Next(100000);
                    c.Test = testString;
                    break;
                }
            }
        }
    }
}