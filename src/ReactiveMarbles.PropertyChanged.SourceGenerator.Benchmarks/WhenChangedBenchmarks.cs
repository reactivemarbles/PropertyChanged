// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Threading.Tasks;

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
        public CompilationUtil Compilation { get; set; }

        [ParamsAllValues]
        public InvocationKind InvocationKind { get; set; }

        [ParamsAllValues]
        public ReceiverKind ReceiverKind { get; set; }

        [Params(Accessibility.Public, Accessibility.Private)]
        public Accessibility Accessibility { get; set; }

        public string UserSource { get; set; }


        [GlobalSetup(Targets = new[] { nameof(Depth1WhenChanged) })]
        public Task Depth1WhenChangedSetup()
        {
            EmptyClassBuilder hostPropertyTypeInfo = new EmptyClassBuilder()
                .WithClassAccess(Accessibility);
            UserSource = new WhenChangedHostBuilder()
                .WithClassAccess(Accessibility)
                .WithPropertyType(hostPropertyTypeInfo)
                .WithInvocation(InvocationKind, x => x.Value)
                .BuildSource();

            Compilation = new CompilationUtil(_ => { });
            return Compilation.Initialize();
        }

        [Benchmark]
        [BenchmarkCategory("Change Depth 1")]
        public void Depth1WhenChanged()
        {
            var generator = Compilation.RunGenerators(out _, out _, out _, out _, new[] { ("usersource.cs", UserSource) });
        }
        [GlobalSetup(Targets = new[] { nameof(Depth2WhenChanged) })]
        public Task Depth2WhenChangedSetup()
        {
            EmptyClassBuilder hostPropertyTypeInfo = new EmptyClassBuilder()
                .WithClassAccess(Accessibility);
            UserSource = new WhenChangedHostBuilder()
                .WithClassAccess(Accessibility)
                .WithPropertyType(hostPropertyTypeInfo)
                .WithInvocation(InvocationKind, x => x.Child.Value)
                .BuildSource();

            Compilation = new CompilationUtil(_ => { });
            return Compilation.Initialize();
        }

        [Benchmark]
        [BenchmarkCategory("Change Depth 2")]
        public void Depth2WhenChanged()
        {
            var generator = Compilation.RunGenerators(out _, out _, out _, out _, new[] { ("usersource.cs", UserSource) });
        }
        [GlobalSetup(Targets = new[] { nameof(Depth10WhenChanged) })]
        public Task Depth10WhenChangedSetup()
        {
            EmptyClassBuilder hostPropertyTypeInfo = new EmptyClassBuilder()
                .WithClassAccess(Accessibility);
            UserSource = new WhenChangedHostBuilder()
                .WithClassAccess(Accessibility)
                .WithPropertyType(hostPropertyTypeInfo)
                .WithInvocation(InvocationKind, x => x.Child.Child.Child.Child.Child.Child.Child.Child.Child.Value)
                .BuildSource();

            Compilation = new CompilationUtil(_ => { });
            return Compilation.Initialize();
        }

        [Benchmark]
        [BenchmarkCategory("Change Depth 10")]
        public void Depth10WhenChanged()
        {
            var generator = Compilation.RunGenerators(out _, out _, out _, out _, new[] { ("usersource.cs", UserSource) });
        }

        [GlobalSetup(Targets = new[] { nameof(Depth20WhenChanged) })]
        public Task Depth20WhenChangedSetup()
        {
            EmptyClassBuilder hostPropertyTypeInfo = new EmptyClassBuilder()
                .WithClassAccess(Accessibility);
            UserSource = new WhenChangedHostBuilder()
                .WithClassAccess(Accessibility)
                .WithPropertyType(hostPropertyTypeInfo)
                .WithInvocation(InvocationKind, x => x.Child.Child.Child.Child.Child.Child.Child.Child.Child.Child.Child.Child.Child.Child.Child.Child.Child.Child.Child.Value)
                .BuildSource();

            Compilation = new CompilationUtil(_ => { });
            return Compilation.Initialize();
        }

        [Benchmark]
        [BenchmarkCategory("Change Depth 20")]
        public void Depth20WhenChanged()
        {
            var generator = Compilation.RunGenerators(out _, out _, out _, out _, new[] { ("usersource.cs", UserSource) });
        }

    }
}
