// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Builders
{
    /// <summary>
    /// Utility methods to assist with compilations.
    /// </summary>
    public static class CompilationUtil
    {
        /// <summary>
        /// Creates a compilation.
        /// </summary>
        /// <param name="sources">The source code to include.</param>
        /// <returns>The created compilation.</returns>
        public static Compilation CreateCompilation(params string[] sources)
        {
            var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);

            return CSharpCompilation.Create(
                assemblyName: "compilation",
                syntaxTrees: sources.Select(x => CSharpSyntaxTree.ParseText(x, new CSharpParseOptions(LanguageVersion.Latest))),
                references: new[]
                {
                    MetadataReference.CreateFromFile(typeof(Observable).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(WhenChangedGenerator).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "mscorlib.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Core.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "netstandard.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Linq.Expressions.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.ObjectModel.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Private.CoreLib.dll")),
                },
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        }

        /// <summary>
        /// Executes the source generators.
        /// </summary>
        /// <param name="compilation">The target compilation.</param>
        /// <param name="diagnostics">The resulting diagnostics.</param>
        /// <param name="generators">The generators to include in the compilation.</param>
        /// <returns>The new compilation after the generators have executed.</returns>
        public static Compilation RunGenerators(Compilation compilation, out ImmutableArray<Diagnostic> diagnostics, params ISourceGenerator[] generators)
        {
            CreateDriver(compilation, generators).RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out diagnostics);
            return outputCompilation;
        }

        private static GeneratorDriver CreateDriver(Compilation compilation, params ISourceGenerator[] generators) =>
            CSharpGeneratorDriver.Create(
                generators: ImmutableArray.Create(generators),
                additionalTexts: ImmutableArray<AdditionalText>.Empty,
                parseOptions: (CSharpParseOptions)compilation.SyntaxTrees.First().Options,
                optionsProvider: null);
    }
}
