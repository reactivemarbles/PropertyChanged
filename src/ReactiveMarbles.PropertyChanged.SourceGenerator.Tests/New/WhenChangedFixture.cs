// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Tests
{
    internal class WhenChangedFixture
    {
        private WhenChangedHostBuilder _hostTypeInfo;
        private Compilation _compilation;
        private Type _hostType;
        private Type _valuePropertyType;

        private WhenChangedFixture(WhenChangedHostBuilder hostTypeInfo, Compilation compilation)
        {
            _hostTypeInfo = hostTypeInfo;
            _compilation = compilation;
        }

        public static WhenChangedFixture Create(WhenChangedHostBuilder hostTypeInfo, params string[] extraSources)
        {
            var sources = extraSources.Prepend(hostTypeInfo.BuildRoot()).ToArray();
            Compilation compilation = CompilationUtil.CreateCompilation(sources);
            return new WhenChangedFixture(hostTypeInfo, compilation);
        }

        public void RunGenerator(out ImmutableArray<Diagnostic> compilationDiagnostics, out ImmutableArray<Diagnostic> generatorDiagnostics, ITestOutputHelper output)
        {
            var newCompilation = CompilationUtil.RunGenerators(_compilation, out generatorDiagnostics, new Generator());
            compilationDiagnostics = newCompilation.GetDiagnostics();
            var compilationErrors = compilationDiagnostics.Where(x => x.Severity == DiagnosticSeverity.Error).Select(x => x.GetMessage());

            if (compilationErrors.Count() > 0)
            {
                var sources = string.Join(Environment.NewLine, newCompilation.SyntaxTrees.Select(x => x.ToString()).Where(x => !x.Contains("The impementation should have been generated.")));
                output.WriteLine(sources);
                throw new XunitException(string.Join('\n', compilationErrors));
            }

            var assembly = GetAssembly(newCompilation);
            _hostType = assembly.GetType(_hostTypeInfo.GetTypeName());
            _valuePropertyType = assembly.GetType(_hostTypeInfo.ValuePropertyTypeName);
        }

        public HostProxy NewHostInstance()
        {
            return new HostProxy(CreateInstance(_hostType));
        }

        public object NewValuePropertyInstance()
        {
            return CreateInstance(_valuePropertyType);
        }

        private static object CreateInstance(Type type)
        {
            return Activator.CreateInstance(type, bindingAttr: BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, null, null);
        }

        private static Assembly GetAssembly(Compilation compilation)
        {
            using var ms = new MemoryStream();
            var result = compilation.Emit(ms);
            ms.Seek(0, SeekOrigin.Begin);
            return Assembly.Load(ms.ToArray());
        }
    }
}
