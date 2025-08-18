// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FluentAssertions;

using Microsoft.CodeAnalysis;

using ReactiveMarbles.PropertyChanged.SourceGenerator.Builders;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Tests;

internal static class TestHelper
{
    public static void CheckDiagnostics(this CompilationUtil compilationUtil, (string FileName, string Source) source, DiagnosticDescriptor expectedDiagnostic)
    {
        compilationUtil.RunGenerators(out var compilationDiagnostics, out var generatorDiagnostics, out var compilation, out var newCompilation, source);
        var compilationErrors = compilationDiagnostics.Where(x => x.Severity == DiagnosticSeverity.Error).Select(x => x.GetMessage()).ToList();

        compilationErrors.Should().BeEmpty();
        generatorDiagnostics.Should().HaveCount(1);
        expectedDiagnostic.Should().Be(generatorDiagnostics[0].Descriptor);
    }
}
