// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Microsoft.CodeAnalysis;
using ReactiveMarbles.PropertyChanged.SourceGenerator.Builders;
using Xunit;
using Xunit.Abstractions;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Tests
{
    /// <summary>
    /// Source generator tets.
    /// </summary>
    public class WhenChangedGeneratorTestsNew
    {
        private readonly ITestOutputHelper _testOutputHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="WhenChangedGeneratorTestsNew"/> class.
        /// </summary>
        /// <param name="testOutputHelper">The output provided by xUnit.</param>
        public WhenChangedGeneratorTestsNew(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        /// <summary>
        /// Tests that no diagnostics are reported when property access modifier is private.
        /// </summary>
        /// <param name="hostContainerTypeAccess">outerClassAccess.</param>
        /// <param name="hostTypeAccess">hostClassAccess.</param>
        /// <param name="propertyTypeAccess">outputClassAccess.</param>
        /// <param name="propertyAccess">propertyAccess.</param>
        [Theory]
        [MemberData(nameof(AccessibilityTestCases.GetValidAccessModifierCombinations), MemberType = typeof(AccessibilityTestCases))]
        public void NoDiagnostics_Roslyn(Accessibility hostContainerTypeAccess, Accessibility hostTypeAccess, Accessibility propertyTypeAccess, Accessibility propertyAccess)
        {
            NoDiagnosticTest(hostContainerTypeAccess, hostTypeAccess, propertyTypeAccess, propertyAccess, true);
        }

        /// <summary>
        /// Tests that no diagnostics are reported when property access modifier is private.
        /// </summary>
        /// <param name="hostContainerTypeAccess">outerClassAccess.</param>
        /// <param name="hostTypeAccess">hostClassAccess.</param>
        /// <param name="propertyTypeAccess">outputClassAccess.</param>
        /// <param name="propertyAccess">propertyAccess.</param>
        [Theory]
        [MemberData(nameof(AccessibilityTestCases.GetValidAccessModifierCombinations), MemberType = typeof(AccessibilityTestCases))]
        public void NoDiagnostics(Accessibility hostContainerTypeAccess, Accessibility hostTypeAccess, Accessibility propertyTypeAccess, Accessibility propertyAccess)
        {
            NoDiagnosticTest(hostContainerTypeAccess, hostTypeAccess, propertyTypeAccess, propertyAccess, false);
        }

        private void NoDiagnosticTest(Accessibility hostContainerTypeAccess, Accessibility hostTypeAccess, Accessibility propertyTypeAccess, Accessibility propertyAccess, bool useRoslyn)
        {
            var hostPropertyTypeInfo = new EmptyClassBuilder()
    .WithClassAccess(propertyTypeAccess);
            var hostTypeInfo = new WhenChangedHostBuilder()
                .WithClassName("Host")
                .WithInvocation(InvocationKind.MemberAccess, ReceiverKind.This, x => x.Child, x => x.Value, (a, b) => b)
                .WithClassAccess(hostTypeAccess)
                .AddNestedClass(hostPropertyTypeInfo)
                .WithPropertyType(hostPropertyTypeInfo)
                .WithPropertyAccess(propertyAccess);
            var hostContainerTypeInfo = new EmptyClassBuilder()
                .WithClassName("HostContainer")
                .WithClassAccess(hostContainerTypeAccess)
                .AddNestedClass(hostTypeInfo);

            var fixture = WhenChangedFixture.Create(hostTypeInfo, _testOutputHelper);
            fixture.RunGenerator(out var compilationDiagnostics, out var generatorDiagnostics, useRoslyn);

            Assert.Empty(generatorDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));
            Assert.Empty(compilationDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));

            var host = fixture.NewHostInstance();
            host.Value = fixture.NewValuePropertyInstance();
            var observable = host.GetWhenChangedObservable(_ => _testOutputHelper.WriteLine(fixture.Sources));
            object value = null;
            observable.Subscribe(x => value = x);
            Assert.Equal(host.Value, value);
            host.Value = fixture.NewValuePropertyInstance();
            Assert.Equal(host.Value, value);
            host.Value = null;
            Assert.Null(value);
        }
    }
}
