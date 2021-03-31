// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

using Microsoft.CodeAnalysis;

using ReactiveMarbles.PropertyChanged.SourceGenerator.Builders;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Benchmarks
{
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [MemoryDiagnoser]
    [MarkdownExporterAttribute.GitHub]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    public class WhenChangedBenchmarks
    {
	    public Compilation Compilation { get; set; }

        [ParamsAllValues]
        public bool IsRoslyn { get; set; }

        [ParamsAllValues]
        public InvocationKind InvocationKind {  get; set; }

        [ParamsAllValues]
        public ReceiverKind ReceiverKind { get; set; }

        [ParamsAllValues]
        public Accessibility Accessibility { get; set; }


        [GlobalSetup(Targets = new[] { nameof(Depth1WhenChanged) })]
        public void Depth1WhenChangedSetup()
        {
            string userSource = new WhenChangedHostBuilder()
                .WithClassAccess(Accessibility)
                .WithInvocation(InvocationKind, ReceiverKind, x => x.Value)
                .BuildSource();

            Compilation = CompilationUtil.CreateCompilation(userSource);
        }

        public void Depth1WhenChanged()
        {
            var newCompilation = CompilationUtil.RunGenerators(Compilation, out var generatorDiagnostics, new Generator() { UseRoslyn = IsRoslyn });
        }
        [GlobalSetup(Targets = new[] { nameof(Depth2WhenChanged) })]
        public void Depth2WhenChangedSetup()
        {
            string userSource = new WhenChangedHostBuilder()
                .WithClassAccess(Accessibility)
                .WithInvocation(InvocationKind, ReceiverKind, x => x.Child.Value)
                .BuildSource();

            Compilation = CompilationUtil.CreateCompilation(userSource);
        }

        public void Depth2WhenChanged()
        {
            var newCompilation = CompilationUtil.RunGenerators(Compilation, out var generatorDiagnostics, new Generator() { UseRoslyn = IsRoslyn });
        }
        [GlobalSetup(Targets = new[] { nameof(Depth10WhenChanged) })]
        public void Depth10WhenChangedSetup()
        {
            string userSource = new WhenChangedHostBuilder()
                .WithClassAccess(Accessibility)
                .WithInvocation(InvocationKind, ReceiverKind, x => x.Child.Child.Child.Child.Child.Child.Child.Child.Child.Value)
                .BuildSource();

            Compilation = CompilationUtil.CreateCompilation(userSource);
        }

        public void Depth10WhenChanged()
        {
            var newCompilation = CompilationUtil.RunGenerators(Compilation, out var generatorDiagnostics, new Generator() { UseRoslyn = IsRoslyn });
        }
        [GlobalSetup(Targets = new[] { nameof(Depth20WhenChanged) })]
        public void Depth20WhenChangedSetup()
        {
            string userSource = new WhenChangedHostBuilder()
                .WithClassAccess(Accessibility)
                .WithInvocation(InvocationKind, ReceiverKind, x => x.Child.Child.Child.Child.Child.Child.Child.Child.Child.Child.Child.Child.Child.Child.Child.Child.Child.Child.Child.Value)
                .BuildSource();

            Compilation = CompilationUtil.CreateCompilation(userSource);
        }

        public void Depth20WhenChanged()
        {
            var newCompilation = CompilationUtil.RunGenerators(Compilation, out var generatorDiagnostics, new Generator() { UseRoslyn = IsRoslyn });
        }

    }
}
