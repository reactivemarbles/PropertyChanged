// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Linq;

using Microsoft.CodeAnalysis;

using ReactiveMarbles.PropertyChanged.SourceGenerator.Builders;

using Xunit;
using Xunit.Abstractions;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Tests
{
    /// <summary>
    /// Source generator tets.
    /// </summary>
    public class BindGeneratorTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="BindGeneratorTests"/> class.
        /// </summary>
        /// <param name="testOutputHelper">The output provided by xUnit.</param>
        public BindGeneratorTests(ITestOutputHelper testOutputHelper)
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
        public void NoDiagnostics(Accessibility hostContainerTypeAccess, Accessibility hostTypeAccess, Accessibility propertyTypeAccess, Accessibility propertyAccess)
        {
            NoDiagnosticTest(hostContainerTypeAccess, hostTypeAccess, propertyTypeAccess, propertyAccess, true);
        }

        private void NoDiagnosticTest(Accessibility hostContainerTypeAccess, Accessibility hostTypeAccess, Accessibility propertyTypeAccess, Accessibility propertyAccess, bool useRoslyn)
        {
            var viewModelHostDetails = new WhenChangedHostBuilder()
                .WithClassAccess(propertyTypeAccess)
                .WithInvocation(InvocationKind.MemberAccess, ReceiverKind.This, x => x.Value)
                .WithPropertyType("string")
                .WithPropertyAccess(propertyAccess)
                .WithClassName("ViewModelHost");

            var hostTypeInfo = new BindHostBuilder()
                .WithClassName("Host")
                .WithTwoWayInvocation(InvocationKind.MemberAccess, ReceiverKind.This, x => x.Value, x => x.Value)
                .WithClassAccess(hostTypeAccess)
                .WithPropertyType("string")
                .WithPropertyAccess(propertyAccess)
                .WithViewModelPropertyType(viewModelHostDetails)
                .AddNestedClass(viewModelHostDetails);

            var hostContainerTypeInfo = new EmptyClassBuilder()
                .WithClassName("HostContainer")
                .WithClassAccess(hostContainerTypeAccess)
                .AddNestedClass(hostTypeInfo);

            var fixture = BindFixture.Create(hostTypeInfo, _testOutputHelper);
            fixture.RunGenerator(out var compilationDiagnostics, out var generatorDiagnostics, useRoslyn);

            Assert.Empty(generatorDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));
            Assert.Empty(compilationDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));

            var host = fixture.NewHostInstance();
            host.Value = fixture.NewViewModelPropertyInstance();
            var disposable = host.GetTwoWayBindSubscription(_ => _testOutputHelper.WriteLine(fixture.Sources));

            var viewModelObservable = host.GetViewModelWhenChangedObservable(_ => _testOutputHelper.WriteLine(fixture.Sources));
            var viewObservable = host.GetViewWhenChangedObservable(_ => _testOutputHelper.WriteLine(fixture.Sources));
            object viewModelValue = null;
            object viewValue = null;
            viewModelObservable.Subscribe(x => viewModelValue = x);
            viewObservable.Subscribe(x => viewValue = x);

            host.ViewModel = fixture.NewViewModelPropertyInstance();
        }
    }
}
