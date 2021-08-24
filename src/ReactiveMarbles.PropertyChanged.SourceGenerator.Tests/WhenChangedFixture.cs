// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.CodeAnalysis;

using ReactiveMarbles.PropertyChanged.SourceGenerator.Builders;

using Xunit.Abstractions;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Tests
{
    internal class WhenChangedFixture
    {
        private readonly WhenChangedHostBuilder _hostTypeInfo;
        private readonly WhenChangedHostBuilder _receiverTypeInfo;
        private readonly Compilation _compilation;
        private readonly ITestOutputHelper _testLogger;
        private Type _hostType;
        private Type _receiverType;
        private Type _valuePropertyType;

        private WhenChangedFixture(WhenChangedHostBuilder hostTypeInfo, WhenChangedHostBuilder receiverTypeInfo, Compilation compilation, ITestOutputHelper testLogger)
        {
            _hostTypeInfo = hostTypeInfo;
            _receiverTypeInfo = receiverTypeInfo;
            _compilation = compilation;
            _testLogger = testLogger;
        }

        public string Sources { get; private set; }

        public static WhenChangedFixture Create(WhenChangedHostBuilder hostTypeInfo, ITestOutputHelper testOutputHelper, params string[] extraSources) => Create(hostTypeInfo, hostTypeInfo, testOutputHelper, extraSources);

        public static WhenChangedFixture Create(WhenChangedHostBuilder hostTypeInfo, WhenChangedHostBuilder receiverTypeInfo, ITestOutputHelper testOutputHelper, params string[] extraSources)
        {
            var sources = extraSources.Prepend(receiverTypeInfo.BuildRoot());
            if (receiverTypeInfo != hostTypeInfo)
            {
                sources = sources.Prepend(hostTypeInfo.BuildRoot());
            }

            var compilation = CompilationUtil.CreateCompilation(sources.ToArray());
            return new(hostTypeInfo, receiverTypeInfo, compilation, testOutputHelper);
        }

        public void RunGenerator(out ImmutableArray<Diagnostic> compilationDiagnostics, out ImmutableArray<Diagnostic> generatorDiagnostics, bool saveCompilation = false, string directory = @"C:\Users\Glenn\source\repos\ConsoleApp8\ConsoleApp8")
        {
            var newCompilation = CompilationUtil.RunGenerators(_compilation, out generatorDiagnostics, new Generator());

            if (saveCompilation)
            {
                SaveCompilation(newCompilation, directory);
            }

            compilationDiagnostics = newCompilation.GetDiagnostics();

            var compilationErrors = compilationDiagnostics.Where(x => x.Severity == DiagnosticSeverity.Error).Select(x => x.GetMessage()).ToList();
            Sources = string.Join(Environment.NewLine, newCompilation.SyntaxTrees.Select(x => x.ToString()).Where(x => !x.Contains("The implementation should have been generated.")));

            if (compilationErrors.Count > 0)
            {
                _testLogger.WriteLine(Sources);
                throw new InvalidOperationException(string.Join('\n', compilationErrors));
            }

            var assembly = GetAssembly(newCompilation);
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

        private static void SaveCompilation(Compilation compilation, string directory)
        {
            foreach (var file in Directory.GetFiles(directory))
            {
                if (!Path.GetExtension(file).Equals(".cs", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (Path.GetFileName(file).Equals("program.cs", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                File.Delete(file);
            }

            var i = 0;
            foreach (var compile in compilation.SyntaxTrees)
            {
                var text = compile.GetText().ToString();

                var fileName = string.IsNullOrWhiteSpace(compile.FilePath) ? $"File{i}.cs" : Path.GetFileName(compile.FilePath);
                var filePath = Path.Combine(directory, fileName);

                File.WriteAllText(filePath, text);
                i++;
            }
        }
    }
}
