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
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;

using ReactiveMarbles.PropertyChanged.SourceGenerator.Builders;

using Xunit.Abstractions;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Tests
{
    internal class BindFixture
    {
        private readonly BindHostBuilder _hostTypeInfo;
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly CompilationUtil _compilationUtil;
        private readonly string[] _extraSources;
        private Type _hostType;
        private Type _hostPropertyType;
        private Type _targetPropertyType;

        private BindFixture(BindHostBuilder hostTypeInfo, ITestOutputHelper testOutputHelper, string[] extraSources)
        {
            _hostTypeInfo = hostTypeInfo;
            _testOutputHelper = testOutputHelper;
            _compilationUtil = new CompilationUtil(x => _testOutputHelper.WriteLine(x));
            _extraSources = extraSources;
        }

        public string Sources { get; private set; }

        public static async Task<BindFixture> Create(BindHostBuilder hostTypeInfo, ITestOutputHelper testOutputHelper, params string[] extraSources)
        {
            var bindFixture = new BindFixture(hostTypeInfo, testOutputHelper, extraSources);
            await bindFixture.Initialize().ConfigureAwait(false);
            return bindFixture;
        }

        public Task Initialize() => _compilationUtil.Initialize();

        public void RunGenerator(BindHostBuilder hostTypeInfo, out ImmutableArray<Diagnostic> compilationDiagnostics, out ImmutableArray<Diagnostic> generatorDiagnostics)
        {
            var sources = _extraSources.Prepend(hostTypeInfo.BuildRoot()).ToArray();

            _compilationUtil.RunGenerators(out compilationDiagnostics, out generatorDiagnostics, out var beforeCompilation, out var afterCompilation, sources.ToArray());

            var compilationErrors = compilationDiagnostics.Where(x => x.Severity == DiagnosticSeverity.Error).Select(x => x.GetMessage()).ToList();
            Sources = string.Join(Environment.NewLine, afterCompilation.SyntaxTrees.Select(x => x.ToString()).Where(x => !x.Contains("The implementation should have been generated.")));
            if (compilationErrors.Count > 0)
            {
                _testOutputHelper.WriteLine(Sources);
                throw new InvalidOperationException(string.Join('\n', compilationErrors));
            }

            var assembly = GetAssembly(afterCompilation);
            _hostType = assembly.GetType(_hostTypeInfo.GetTypeName());
            _hostPropertyType = assembly.GetType(_hostTypeInfo.HostPropertyTypeName);
            _targetPropertyType = assembly.GetType(_hostTypeInfo.TargetPropertyTypeName);
        }

        public BindHostProxy NewHostInstance() => new(CreateInstance(_hostType));

        public WhenChangedHostProxy NewViewModelPropertyInstance() => new(CreateInstance(_hostPropertyType));

        public object NewValuePropertyInstance() => CreateInstance(_targetPropertyType);

        private static object CreateInstance(Type type) => Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, null, null);

        private static Assembly GetAssembly(Compilation compilation)
        {
            using var ms = new MemoryStream();
            var result = compilation.Emit(ms);
            ms.Seek(0, SeekOrigin.Begin);
            return Assembly.Load(ms.ToArray());
        }
    }
}
