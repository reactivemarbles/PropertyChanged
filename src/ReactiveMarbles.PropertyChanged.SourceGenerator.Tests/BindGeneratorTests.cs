// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
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
        /// Gets test cases.
        /// </summary>
        /// <returns>Test cases.</returns>
        public static IEnumerable<object[]> GetValidAccessModifierCombinations()
        {
            var testCases = AccessibilityTestCases.GetValidAccessModifierCombinations().Select(x => ((Accessibility)x[0], (Accessibility)x[1], (Accessibility)x[2], (Accessibility)x[3]));

            var vmPropertiesAccessList = new[] { Accessibility.Public, Accessibility.Internal, Accessibility.ProtectedOrInternal };

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
        public void NoDiagnostics(Accessibility hostContainerTypeAccess, Accessibility hostTypeAccess, Accessibility hostPropertiesAccess, Accessibility vmTypeAccess, Accessibility vmPropertiesAccess)
        {
            var viewModelHostDetails = new WhenChangedHostBuilder()
                .WithClassAccess(vmTypeAccess)
                .WithInvocation(InvocationKind.MemberAccess, ReceiverKind.This, x => x.Value)
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

            var fixture = BindFixture.Create(hostTypeInfo, _testOutputHelper);
            fixture.RunGenerator(out var compilationDiagnostics, out var generatorDiagnostics, true);

            Assert.Empty(generatorDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));
            Assert.Empty(compilationDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));

            var host = fixture.NewHostInstance();
            host.ViewModel = fixture.NewViewModelPropertyInstance();
            var disposable = host.GetTwoWayBindSubscription(_ => _testOutputHelper.WriteLine(fixture.Sources));

            var viewModelObservable = host.GetViewModelWhenChangedObservable(_ => _testOutputHelper.WriteLine(fixture.Sources));
            var viewObservable = host.GetWhenChangedObservable(_ => _testOutputHelper.WriteLine(fixture.Sources));
            object viewModelValue = null;
            object viewValue = null;
            viewModelObservable.Subscribe(x => viewModelValue = x);
            viewObservable.Subscribe(x => viewValue = x);

            host.Value = "test";

            Assert.Equal("test", host.ViewModel.Value);

            host.Value = "Test2";
            Assert.Equal("Test2", host.ViewModel.Value);

            host.ViewModel.Value = "Test3";
            Assert.Equal("Test3", host.Value);
        }
    }
}
