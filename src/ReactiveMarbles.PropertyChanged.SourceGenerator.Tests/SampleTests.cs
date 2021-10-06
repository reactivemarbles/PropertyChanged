// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReactiveMarbles.PropertyChanged.SourceGenerator.Builders;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Tests;

/// <summary>
/// Tests the sample project.
/// </summary>
public class SampleTests
{
    private readonly CompilationUtil _compilationUtil;

    /// <summary>
    /// Initializes a new instance of the <see cref="SampleTests"/> class.
    /// </summary>
    public SampleTests() => _compilationUtil = new(x => TestContext?.WriteLine(x));

    /// <summary>
    /// Gets or sets the test context.
    /// </summary>
    public TestContext TestContext { get; set; }

    /// <summary>
    /// Initializes the test.
    /// </summary>
    /// <returns>A task.</returns>
    [TestInitialize]
    public Task InitializeAsync() => _compilationUtil.Initialize();

    /// <summary>
    /// Runs the samples porject.
    /// </summary>
    [TestMethod]
    public void TestSample()
    {
        var files = Directory.GetFiles("../../../../ReactiveMarbles.PropertyChanged.SourceGenerator.Sample/", "*.cs", new EnumerationOptions() { RecurseSubdirectories = true, IgnoreInaccessible = true, MatchType = MatchType.Simple })
            .Where(x => !x.Contains("obj" + Path.DirectorySeparatorChar, StringComparison.InvariantCulture));

        var sources = files.Select(x => (x, File.ReadAllText(x))).ToArray();

        _compilationUtil.RunGenerators(out var compilationDiagnostics, out var generatorDiagnostics, out var beforeCompilation, out var afterCompilation, sources);

        var compilationErrors = compilationDiagnostics.Where(x => x.Severity == DiagnosticSeverity.Error).Select(x => x.GetMessage()).ToList();
        if (compilationErrors.Count > 0)
        {
            var outputSoruces = string.Join(Environment.NewLine, afterCompilation.SyntaxTrees.Select(x => x.ToString()).Where(x => !x.Contains("The implementation should have been generated.")));
            TestContext.WriteLine(outputSoruces);
            throw new InvalidOperationException(string.Join('\n', compilationErrors));
        }

        generatorDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning).Should().BeEmpty();
        compilationDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning).Should().BeEmpty();
    }
}
