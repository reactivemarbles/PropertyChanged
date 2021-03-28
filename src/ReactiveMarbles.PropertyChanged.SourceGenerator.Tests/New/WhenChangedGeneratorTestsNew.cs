// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Abstractions;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Tests
{
    /// <summary>
    /// Source generator tets.
    /// </summary>
    public class WhenChangedGeneratorTestsNew
    {
        private readonly ITestOutputHelper _output;

        /// <summary>
        /// Initializes a new instance of the <see cref="WhenChangedGeneratorTestsNew"/> class.
        /// </summary>
        /// <param name="output">The output provided by xUnit.</param>
        public WhenChangedGeneratorTestsNew(ITestOutputHelper output)
        {
            _output = output;
        }

        /// <summary>
        /// Gets the testing data.
        /// </summary>
        /// <returns>The source for a data theory.</returns>
        public static IEnumerable<object[]> GetData()
        {
            var hostContainerTypeAccessList = new[] { Accessibility.Public, Accessibility.Internal };
            var hostTypeAccessList = new[] { Accessibility.Private, Accessibility.ProtectedAndInternal, Accessibility.Protected, Accessibility.Internal, Accessibility.ProtectedOrInternal, Accessibility.Public };
            var propertyTypeAccessList = new[] { Accessibility.Private, Accessibility.ProtectedAndInternal, Accessibility.Protected, Accessibility.Internal, Accessibility.ProtectedOrInternal, Accessibility.Public };
            var propertyAccessList = new[] { Accessibility.Private, Accessibility.ProtectedAndInternal, Accessibility.Protected, Accessibility.Internal, Accessibility.ProtectedOrInternal, Accessibility.Public };

            return
                from hostContainerTypeAccess in hostContainerTypeAccessList
                from hostTypeAccess in hostTypeAccessList
                from propertyTypeAccess in propertyTypeAccessList
                from propertyAccess in propertyAccessList
                where TestCaseUtil.ValidateAccessModifierCombination(hostContainerTypeAccess, hostTypeAccess, propertyTypeAccess, propertyAccess)
                select new object[] { hostContainerTypeAccess, hostTypeAccess, propertyTypeAccess, propertyAccess };
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

            var fixture = WhenChangedFixture.Create(hostTypeInfo);
            fixture.RunGenerator(out var compilationDiagnostics, out var generatorDiagnostics, _output);

            Assert.Empty(generatorDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));
            Assert.Empty(compilationDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));
        }
    }
}
