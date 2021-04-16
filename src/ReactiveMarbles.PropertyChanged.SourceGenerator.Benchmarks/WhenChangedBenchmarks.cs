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
        public InvocationKind InvocationKind {  get; set; }

        [ParamsAllValues]
        public ReceiverKind ReceiverKind { get; set; }

        [Params(Accessibility.Public, Accessibility.Private)]
        public Accessibility Accessibility { get; set; }


        [GlobalSetup(Targets = new[] { nameof(Depth1WhenChangedRoslyn) })]
        public void Depth1WhenChangedSetupRoslyn()
        {
            var hostPropertyTypeInfo = new EmptyClassBuilder()
                .WithClassAccess(Accessibility);
            string userSource = new WhenChangedHostBuilder()
                .WithClassAccess(Accessibility)
                .WithPropertyType(hostPropertyTypeInfo)
                .WithInvocation(InvocationKind, ReceiverKind, x => x.Value)
                .BuildSource();

            Compilation = CompilationUtil.CreateCompilation(userSource);
        }

        [Benchmark]
        [BenchmarkCategory("Change Depth 1")]
        public void Depth1WhenChangedRoslyn()
        {
            _ = CompilationUtil.RunGenerators(Compilation, out _, new Generator(true));
        }
        [GlobalSetup(Targets = new[] { nameof(Depth1WhenChangedStringBuilder) })]
        public void Depth1WhenChangedSetupStringBuilder()
        {
            var hostPropertyTypeInfo = new EmptyClassBuilder()
                .WithClassAccess(Accessibility);
            string userSource = new WhenChangedHostBuilder()
                .WithClassAccess(Accessibility)
                .WithPropertyType(hostPropertyTypeInfo)
                .WithInvocation(InvocationKind, ReceiverKind, x => x.Value)
                .BuildSource();

            Compilation = CompilationUtil.CreateCompilation(userSource);
        }

        [Benchmark]
        [BenchmarkCategory("Change Depth 1")]
        public void Depth1WhenChangedStringBuilder()
        {
            _ = CompilationUtil.RunGenerators(Compilation, out _, new Generator(false));
        }
        [GlobalSetup(Targets = new[] { nameof(Depth2WhenChangedRoslyn) })]
        public void Depth2WhenChangedSetupRoslyn()
        {
            var hostPropertyTypeInfo = new EmptyClassBuilder()
                .WithClassAccess(Accessibility);
            string userSource = new WhenChangedHostBuilder()
                .WithClassAccess(Accessibility)
                .WithPropertyType(hostPropertyTypeInfo)
                .WithInvocation(InvocationKind, ReceiverKind, x => x.Child.Value)
                .BuildSource();

            Compilation = CompilationUtil.CreateCompilation(userSource);
        }

        [Benchmark]
        [BenchmarkCategory("Change Depth 2")]
        public void Depth2WhenChangedRoslyn()
        {
            _ = CompilationUtil.RunGenerators(Compilation, out _, new Generator(true));
        }
        [GlobalSetup(Targets = new[] { nameof(Depth2WhenChangedStringBuilder) })]
        public void Depth2WhenChangedSetupStringBuilder()
        {
            var hostPropertyTypeInfo = new EmptyClassBuilder()
                .WithClassAccess(Accessibility);
            string userSource = new WhenChangedHostBuilder()
                .WithClassAccess(Accessibility)
                .WithPropertyType(hostPropertyTypeInfo)
                .WithInvocation(InvocationKind, ReceiverKind, x => x.Child.Value)
                .BuildSource();

            Compilation = CompilationUtil.CreateCompilation(userSource);
        }

        [Benchmark]
        [BenchmarkCategory("Change Depth 2")]
        public void Depth2WhenChangedStringBuilder()
        {
            _ = CompilationUtil.RunGenerators(Compilation, out _, new Generator(false));
        }
        [GlobalSetup(Targets = new[] { nameof(Depth10WhenChangedRoslyn) })]
        public void Depth10WhenChangedSetupRoslyn()
        {
            var hostPropertyTypeInfo = new EmptyClassBuilder()
                .WithClassAccess(Accessibility);
            string userSource = new WhenChangedHostBuilder()
                .WithClassAccess(Accessibility)
                .WithPropertyType(hostPropertyTypeInfo)
                .WithInvocation(InvocationKind, ReceiverKind, x => x.Child.Child.Child.Child.Child.Child.Child.Child.Child.Value)
                .BuildSource();

            Compilation = CompilationUtil.CreateCompilation(userSource);
        }

        [Benchmark]
        [BenchmarkCategory("Change Depth 10")]
        public void Depth10WhenChangedRoslyn()
        {
            _ = CompilationUtil.RunGenerators(Compilation, out _, new Generator(true));
        }
        [GlobalSetup(Targets = new[] { nameof(Depth10WhenChangedStringBuilder) })]
        public void Depth10WhenChangedSetupStringBuilder()
        {
            var hostPropertyTypeInfo = new EmptyClassBuilder()
                .WithClassAccess(Accessibility);
            string userSource = new WhenChangedHostBuilder()
                .WithClassAccess(Accessibility)
                .WithPropertyType(hostPropertyTypeInfo)
                .WithInvocation(InvocationKind, ReceiverKind, x => x.Child.Child.Child.Child.Child.Child.Child.Child.Child.Value)
                .BuildSource();

            Compilation = CompilationUtil.CreateCompilation(userSource);
        }

        [Benchmark]
        [BenchmarkCategory("Change Depth 10")]
        public void Depth10WhenChangedStringBuilder()
        {
            _ = CompilationUtil.RunGenerators(Compilation, out _, new Generator(false));
        }
        [GlobalSetup(Targets = new[] { nameof(Depth20WhenChangedRoslyn) })]
        public void Depth20WhenChangedSetupRoslyn()
        {
            var hostPropertyTypeInfo = new EmptyClassBuilder()
                .WithClassAccess(Accessibility);
            string userSource = new WhenChangedHostBuilder()
                .WithClassAccess(Accessibility)
                .WithPropertyType(hostPropertyTypeInfo)
                .WithInvocation(InvocationKind, ReceiverKind, x => x.Child.Child.Child.Child.Child.Child.Child.Child.Child.Child.Child.Child.Child.Child.Child.Child.Child.Child.Child.Value)
                .BuildSource();

            Compilation = CompilationUtil.CreateCompilation(userSource);
        }

        [Benchmark]
        [BenchmarkCategory("Change Depth 20")]
        public void Depth20WhenChangedRoslyn()
        {
            _ = CompilationUtil.RunGenerators(Compilation, out _, new Generator(true));
        }
        [GlobalSetup(Targets = new[] { nameof(Depth20WhenChangedStringBuilder) })]
        public void Depth20WhenChangedSetupStringBuilder()
        {
            var hostPropertyTypeInfo = new EmptyClassBuilder()
                .WithClassAccess(Accessibility);
            string userSource = new WhenChangedHostBuilder()
                .WithClassAccess(Accessibility)
                .WithPropertyType(hostPropertyTypeInfo)
                .WithInvocation(InvocationKind, ReceiverKind, x => x.Child.Child.Child.Child.Child.Child.Child.Child.Child.Child.Child.Child.Child.Child.Child.Child.Child.Child.Child.Value)
                .BuildSource();

            Compilation = CompilationUtil.CreateCompilation(userSource);
        }

        [Benchmark]
        [BenchmarkCategory("Change Depth 20")]
        public void Depth20WhenChangedStringBuilder()
        {
            _ = CompilationUtil.RunGenerators(Compilation, out _, new Generator(false));
        }

    }
}
