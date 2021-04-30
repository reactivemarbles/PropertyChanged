// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using FluentAssertions;

using Microsoft.CodeAnalysis;

using ReactiveMarbles.PropertyChanged.SourceGenerator.Builders;

using Xunit;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Tests
{
    /// <summary>
    /// Bind generator tests.
    /// </summary>
    public partial class BindGeneratorTests
    {
        /// <summary>
        /// Gets test cases.
        /// </summary>
        /// <returns>Test cases.</returns>
        public static IEnumerable<object[]> GetValidAccessModifierCombinations()
        {
            var testCases = AccessibilityTestCases.GetValidAccessModifierCombinations().Select(x => ((Accessibility)x[0], (Accessibility)x[1], (Accessibility)x[2], (Accessibility)x[3]));

            var vmPropertiesAccessList = new[] { Accessibility.Internal };

            foreach (var (hostContainerType, hostTypeAccess, vmTypeAccess, hostPropertiesAccess) in testCases)
            {
                foreach (var vmPropertiesAccess in vmPropertiesAccessList)
                {
                    yield return new object[] { hostContainerType, hostTypeAccess, hostPropertiesAccess, vmTypeAccess, vmPropertiesAccess };
                }
            }
        }

        /// <summary>
        /// Make sure all possible combinations of access modifiers result in successful generation.
        /// </summary>
        /// <param name="hostContainerTypeAccess">The access modifier of the type containing the host type.</param>
        /// <param name="hostTypeAccess">The access modifier of the host type.</param>
        /// <param name="hostPropertiesAccess">The access modifier of the properties in the host.</param>
        /// <param name="vmTypeAccess">The access modifier of the view model type.</param>
        /// <param name="vmPropertiesAccess">The access modifier of the properties in the view model.</param>
        [Theory]
        [MemberData(nameof(AccessibilityTestCases.GetValidAccessModifierCombinations))]
        public void NoDiagnostics_TwoWayBind(Accessibility hostContainerTypeAccess, Accessibility hostTypeAccess, Accessibility hostPropertiesAccess, Accessibility vmTypeAccess, Accessibility vmPropertiesAccess)
        {
            var viewModelHostDetails = new WhenChangedHostBuilder()
                .WithClassAccess(vmTypeAccess)
                .WithInvocation(InvocationKind.MemberAccess, x => x.Value)
                .WithPropertyType("string")
                .WithPropertyAccess(vmPropertiesAccess)
                .WithClassName("ViewModelHost");

            var hostTypeInfo = new BindHostBuilder()
                .WithClassName("Host")
                .WithTwoWayInvocation(InvocationKind.MemberAccess, ReceiverKind.This, x => x.Value, x => x.Value)
                .WithClassAccess(hostTypeAccess)
                .WithPropertyType("string")
                .WithPropertyAccess(hostPropertiesAccess)
                .WithViewModelPropertyAccess(hostPropertiesAccess)
                .WithViewModelPropertyType(viewModelHostDetails)
                .AddNestedClass(viewModelHostDetails);

            var hostContainerTypeInfo = new EmptyClassBuilder()
                .WithClassName("HostContainer")
                .WithClassAccess(hostContainerTypeAccess)
                .AddNestedClass(hostTypeInfo);

            var fixture = BindFixture.Create(hostTypeInfo, TestContext);
            fixture.RunGenerator(out var compilationDiagnostics, out var generatorDiagnostics, saveCompilation: true);

            generatorDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning).Should().BeEmpty();
            compilationDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning).Should().BeEmpty();

            var host = fixture.NewHostInstance();
            host.ViewModel = fixture.NewViewModelPropertyInstance();
            var disposable = host.GetTwoWayBindSubscription(_ => TestContext.WriteLine(fixture.Sources));

            var viewModelObservable = host.GetViewModelWhenChangedObservable(_ => TestContext.WriteLine(fixture.Sources));
            var viewObservable = host.GetWhenChangedObservable(_ => TestContext.WriteLine(fixture.Sources));
            object viewModelValue = null;
            object viewValue = null;
            viewModelObservable.Subscribe(x => viewModelValue = x);
            viewObservable.Subscribe(x => viewValue = x);

            host.Value = "test";
            host.ViewModel.Value.Should().Be("test");

            host.Value = "Test2";
            host.ViewModel.Value.Should().Be("Test2");

            host.ViewModel.Value = "Test3";
            host.Value.Should().Be("Test3");
        }

        /// <summary>
        /// Make sure all possible combinations of access modifiers result in successful generation.
        /// </summary>
        /// <param name="hostContainerTypeAccess">The access modifier of the type containing the host type.</param>
        /// <param name="hostTypeAccess">The access modifier of the host type.</param>
        /// <param name="hostPropertiesAccess">The access modifier of the properties in the host.</param>
        /// <param name="vmTypeAccess">The access modifier of the view model type.</param>
        /// <param name="vmPropertiesAccess">The access modifier of the properties in the view model.</param>
        [Theory]
        [MemberData(nameof(AccessibilityTestCases.GetValidAccessModifierCombinations))]
        public void NoDiagnostics_OneWayBind(Accessibility hostContainerTypeAccess, Accessibility hostTypeAccess, Accessibility hostPropertiesAccess, Accessibility vmTypeAccess, Accessibility vmPropertiesAccess)
        {
            var viewModelHostDetails = new WhenChangedHostBuilder()
                .WithClassAccess(vmTypeAccess)
                .WithInvocation(InvocationKind.MemberAccess, x => x.Value)
                .WithPropertyType("string")
                .WithPropertyAccess(vmPropertiesAccess)
                .WithClassName("ViewModelHost");

            var hostTypeInfo = new BindHostBuilder()
                .WithClassName("Host")
                .WithOneWayInvocation(InvocationKind.MemberAccess, ReceiverKind.This, x => x.Value, x => x.Value)
                .WithClassAccess(hostTypeAccess)
                .WithPropertyType("string")
                .WithPropertyAccess(hostPropertiesAccess)
                .WithViewModelPropertyAccess(hostPropertiesAccess)
                .WithViewModelPropertyType(viewModelHostDetails)
                .AddNestedClass(viewModelHostDetails);

            var hostContainerTypeInfo = new EmptyClassBuilder()
                .WithClassName("HostContainer")
                .WithClassAccess(hostContainerTypeAccess)
                .AddNestedClass(hostTypeInfo);

            var fixture = BindFixture.Create(hostTypeInfo, TestContext);
            fixture.RunGenerator(out var compilationDiagnostics, out var generatorDiagnostics, saveCompilation: false);

            generatorDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning).Should().BeEmpty();
            compilationDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning).Should().BeEmpty();

            var host = fixture.NewHostInstance();
            host.ViewModel = fixture.NewViewModelPropertyInstance();
            var disposable = host.GetOneWayBindSubscription(_ => TestContext.WriteLine(fixture.Sources));

            var viewModelObservable = host.GetViewModelWhenChangedObservable(_ => TestContext.WriteLine(fixture.Sources));
            var viewObservable = host.GetWhenChangedObservable(_ => TestContext.WriteLine(fixture.Sources));
            object viewModelValue = null;
            object viewValue = null;
            viewModelObservable.Subscribe(x => viewModelValue = x);
            viewObservable.Subscribe(x => viewValue = x);

            host.Value = "test";
        }
    }
}
