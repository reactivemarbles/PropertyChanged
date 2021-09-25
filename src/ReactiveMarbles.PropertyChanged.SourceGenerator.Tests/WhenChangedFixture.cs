// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using ReactiveMarbles.PropertyChanged.SourceGenerator.Builders;

using Xunit.Abstractions;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Tests
{
    internal class WhenChangedFixture
    {
        private readonly WhenChangedHostBuilder _hostTypeInfo;
        private readonly WhenChangedHostBuilder _receiverTypeInfo;
        private readonly ITestOutputHelper _testLogger;
        private readonly CompilationUtil _compilationUtil;
        private readonly string[] _extraSources;
        private Type _hostType;
        private Type _receiverType;
        private Type _valuePropertyType;

        private WhenChangedFixture(WhenChangedHostBuilder hostTypeInfo, WhenChangedHostBuilder receiverTypeInfo, ITestOutputHelper testLogger, string[] extraSources)
        {
            _hostTypeInfo = hostTypeInfo;
            _receiverTypeInfo = receiverTypeInfo;
            _testLogger = testLogger;
            _compilationUtil = new CompilationUtil(x => testLogger.WriteLine(x));
            _extraSources = extraSources;
        }

        public string Sources { get; private set; }

        public static Task<WhenChangedFixture> Create(WhenChangedHostBuilder hostTypeInfo, ITestOutputHelper testOutputHelper, params string[] extraSources) =>
            Create(hostTypeInfo, hostTypeInfo, testOutputHelper, extraSources);

        public static async Task<WhenChangedFixture> Create(WhenChangedHostBuilder hostTypeInfo, WhenChangedHostBuilder receiverTypeInfo, ITestOutputHelper testOutputHelper, params string[] extraSources)
        {
            var fixture = new WhenChangedFixture(hostTypeInfo, receiverTypeInfo, testOutputHelper, extraSources);
            await fixture.Initialize().ConfigureAwait(false);
            return fixture;
        }

        public Task Initialize() => _compilationUtil.Initialize();

        public void RunGenerator(out ImmutableArray<Diagnostic> compilationDiagnostics, out ImmutableArray<Diagnostic> generatorDiagnostics)
        {
            var sources = _extraSources.Prepend(_receiverTypeInfo.BuildRoot());
            if (_receiverTypeInfo != _hostTypeInfo)
            {
                sources = sources.Prepend(_hostTypeInfo.BuildRoot());
            }

            _compilationUtil.RunGenerators(out compilationDiagnostics, out generatorDiagnostics, out var beforeCompilation, out var afterCompilation, sources.ToArray());

            var compilationErrors = compilationDiagnostics.Where(x => x.Severity == DiagnosticSeverity.Error).Select(x => x.GetMessage()).ToList();
            Sources = string.Join(Environment.NewLine, afterCompilation.SyntaxTrees.Select(x => x.ToString()).Where(x => !x.Contains("The implementation should have been generated.")));

            if (compilationErrors.Count > 0)
            {
                _testLogger.WriteLine(Sources);
                throw new InvalidOperationException(string.Join('\n', compilationErrors));
            }

            var assembly = GetAssembly(afterCompilation);
            _hostType = assembly.GetType(_hostTypeInfo.GetTypeName());
            _receiverType = assembly.GetType(_receiverTypeInfo.GetTypeName());
            _valuePropertyType = assembly.GetType(_receiverTypeInfo.ValuePropertyTypeName);
        }

        public WhenChangedHostProxy NewHostInstance() => new(CreateInstance(_hostType));

        public WhenChangedHostProxy NewReceiverInstance() => new(CreateInstance(_receiverType));

        public object NewValuePropertyInstance() => CreateInstance(_valuePropertyType);

        private static object CreateInstance(Type type) => Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, null, null);

        private static Assembly GetAssembly(Compilation compilation)
        {
            using var ms = new MemoryStream();
            compilation.Emit(ms);
            ms.Seek(0, SeekOrigin.Begin);
            return Assembly.Load(ms.ToArray());
        }
    }
}
