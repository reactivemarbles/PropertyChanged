// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Tests
{
    /// <summary>
    /// Tests for the generators in general.
    /// </summary>
    public abstract class GeneratorTestBase
    {
        /// <summary>
        /// Sets the property.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The value.</param>
        protected static void SetProperty(object target, string propertyName, object value)
        {
            target.GetType().InvokeMember(
                propertyName,
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null,
                target,
                new object[] { value });
        }

        /// <summary>
        /// Gets the assembly.
        /// </summary>
        /// <param name="compilation">The compilation.</param>
        /// <returns>The assembly associated with the compilation.</returns>
        protected static Assembly GetAssembly(Compilation compilation)
        {
            using var ms = new MemoryStream();
            var result = compilation.Emit(ms);
            ms.Seek(0, SeekOrigin.Begin);
            return Assembly.Load(ms.ToArray());
        }

        /// <summary>
        /// Creates the compilation.
        /// </summary>
        /// <param name="sources">The sources.</param>
        /// <returns>The compilation.</returns>
        protected static Compilation CreateCompilation(params string[] sources)
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
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Console.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "netstandard.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Linq.Expressions.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.ObjectModel.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Private.CoreLib.dll")),
                },
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    .WithSpecificDiagnosticOptions(new[] { new KeyValuePair<string, ReportDiagnostic>("1061", ReportDiagnostic.Suppress) }));
        }

        /// <summary>
        /// Creates the driver.
        /// </summary>
        /// <param name="compilation">The compilation.</param>
        /// <param name="generators">The generators.</param>
        /// <returns>The generator driver.</returns>
        protected static GeneratorDriver CreateDriver(Compilation compilation, params ISourceGenerator[] generators) =>
            CSharpGeneratorDriver.Create(
                generators: ImmutableArray.Create(generators),
                additionalTexts: ImmutableArray<AdditionalText>.Empty,
                parseOptions: (CSharpParseOptions)compilation.SyntaxTrees.First().Options,
                optionsProvider: null);

        /// <summary>
        /// Runs the generators.
        /// </summary>
        /// <param name="compilation">The compilation.</param>
        /// <param name="diagnostics">The diagnostics.</param>
        /// <param name="generators">The generators.</param>
        /// <returns>The compilation for builder use.</returns>
        protected static Compilation RunGenerators(Compilation compilation, out ImmutableArray<Diagnostic> diagnostics, params ISourceGenerator[] generators)
        {
            CreateDriver(compilation, generators).RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out diagnostics);
            return outputCompilation;
        }

        /// <summary>
        /// Creates the instance.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The created object.</returns>
        protected static object CreateInstance(Type type)
        {
            return Activator.CreateInstance(type, bindingAttr: BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, null, null);
        }
    }
}
