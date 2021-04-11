// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

using BenchmarkDotNet.Running;

using ReactiveMarbles.PropertyChanged.SourceGenerator.Builders;

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
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
            ////var benchmark = new WhenChangedBenchmarks();
            ////benchmark.InvocationKind = InvocationKind.MemberAccess;
            ////benchmark.Accessibility = Microsoft.CodeAnalysis.Accessibility.Public;
            ////benchmark.IsRoslyn = true;
            ////benchmark.Depth10WhenChangedSetup();
            ////benchmark.Depth10WhenChanged();
        }
    }
}
