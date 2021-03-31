// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

using BenchmarkDotNet.Running;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Benchmarks
{
    /// <summary>
    /// Class which hosts the main execution point.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point into the benchmarking application.
        /// </summary>
        /// <param name="args">Arguments from the command line.</param>
        public static void Main(string[] args)
        {
            ////BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

            var benchmark = new OldWhenChangedBenchmarks();
            benchmark.Depth = 100;
            benchmark.InvocationKind = InvocationKind.MemberAccess;
            benchmark.IsRoslyn = true;
            benchmark.ExpressionForm = ExpressionForm.Inline;
            benchmark.SetupBasic();
            benchmark.Basic();
        }
    }
}
