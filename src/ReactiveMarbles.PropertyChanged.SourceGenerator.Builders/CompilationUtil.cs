// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using ICSharpCode.Decompiler.Metadata;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using NuGet.LibraryModel;
using NuGet.Versioning;

using ReactiveMarbles.NuGet.Helpers;
using ReactiveMarbles.SourceGenerator.TestNuGetHelper.Compilation;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Builders;

/// <summary>
/// Utility methods to assist with compilations.
/// </summary>
public sealed class CompilationUtil : IDisposable
{
#pragma warning disable CS0618 // Type or member is obsolete
    private readonly LibraryRange _reactiveLibrary = new("System.Reactive", VersionRange.AllStableFloating, LibraryDependencyTarget.Package);
#pragma warning restore CS0618 // Type or member is obsolete

    private readonly SourceGeneratorUtility _sourceGeneratorUtility;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompilationUtil"/> class.
    /// </summary>
    /// <param name="outputHelper">Outputs any warning text.</param>
    public CompilationUtil(Action<string> outputHelper) => _sourceGeneratorUtility = new SourceGeneratorUtility(outputHelper);

    private EventBuilderCompiler? EventCompiler { get; set; }

    /// <summary>
    /// Initializes.
    /// </summary>
    /// <returns>Task to monitor the progress.</returns>
    public async Task Initialize()
    {
        var targetFrameworks = "netstandard2.0".ToFrameworks();

        var inputGroup = await NuGetPackageHelper.DownloadPackageFilesAndFolder(_reactiveLibrary, targetFrameworks, packageOutputDirectory: null).ConfigureAwait(false);

        var framework = targetFrameworks[0];
        EventCompiler = new(inputGroup, inputGroup, framework);
    }

    /// <summary>
    /// Executes the source generators.
    /// </summary>
    /// <param name="compilationDiagnostics">The resulting diagnostics from compilation.</param>
    /// <param name="generatorDiagnostics">The resulting diagnostics from generation.</param>
    /// <param name="beforeGeneratorCompilation">The compiler before generation.</param>
    /// <param name="afterGeneratorCompilation">The compiler after generation.</param>
    /// <param name="sources">The source code.</param>
    /// <returns>The generator.</returns>
    public GeneratorDriver RunGenerators(out ImmutableArray<Diagnostic> compilationDiagnostics, out ImmutableArray<Diagnostic> generatorDiagnostics, out Compilation beforeGeneratorCompilation, out Compilation afterGeneratorCompilation, params (string FileName, string Source)[] sources)
    {
        if (EventCompiler is null)
        {
            throw new InvalidOperationException("Must have valid compiler instance.");
        }

        _sourceGeneratorUtility.RunGenerator<Generator>(EventCompiler, out compilationDiagnostics, out generatorDiagnostics, out var driver, out beforeGeneratorCompilation, out afterGeneratorCompilation, sources);
        return driver;
    }

    /// <inheritdoc />
    public void Dispose() => EventCompiler?.Dispose();
}
