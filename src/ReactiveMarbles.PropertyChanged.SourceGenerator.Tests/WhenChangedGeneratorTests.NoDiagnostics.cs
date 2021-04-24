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
    public partial class WhenChangedGeneratorTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="WhenChangedGeneratorTests"/> class.
        /// </summary>
        /// <param name="testOutputHelper">The output provided by xUnit.</param>
        public WhenChangedGeneratorTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        /// <summary>
        /// Make sure namespaces are handled correctly.
        /// </summary>
        /// <param name="hostTypeNamespace">The namespace name containing the host.</param>
        /// <param name="hostPropertyTypeNamespace">The namespace name containing the property type.</param>
        [Theory]
        [InlineData(null, null)]
        [InlineData("HostNamespace", "CustomNamespace")]
        [InlineData("HostNamespace", null)]
        [InlineData(null, "CustomNamespace")]
        public void NoDiagnostics_Namespaces(string hostTypeNamespace, string hostPropertyTypeNamespace)
        {
            var hostPropertyTypeInfo = new EmptyClassBuilder()
                .WithClassAccess(Accessibility.Public)
                .WithNamespace(hostPropertyTypeNamespace);
            var hostTypeInfo = new WhenChangedHostBuilder()
                .WithClassName("Host")
                .WithInvocation(InvocationKind.MemberAccess, ReceiverKind.This, x => x.Child, x => x.Value, (a, b) => "result" + a + b)
                .WithClassAccess(Accessibility.Public)
                .WithPropertyType(hostPropertyTypeInfo)
                .WithPropertyAccess(Accessibility.Public)
                .WithNamespace(hostTypeNamespace);

            var fixture = WhenChangedFixture.Create(hostTypeInfo, _testOutputHelper, hostPropertyTypeInfo.BuildRoot());
            fixture.RunGenerator(out var compilationDiagnostics, out var generatorDiagnostics, useRoslyn: true, saveCompilation: false);

            Assert.Empty(generatorDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));
            Assert.Empty(compilationDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));

            string Convert(WhenChangedHostProxy host)
            {
                return "result" + host.Child + host.Value;
            }

            var host = fixture.NewHostInstance();
            host.Value = fixture.NewValuePropertyInstance();
            var observable = host.GetWhenChangedObservable(_ => _testOutputHelper.WriteLine(fixture.Sources));
            object value = null;
            observable.Subscribe(x => value = x);
            Assert.Equal(Convert(host), value);
            host.Value = fixture.NewValuePropertyInstance();
            Assert.Equal(Convert(host), value);
            host.Value = null;
            Assert.Equal(Convert(host), value);
        }

        /// <summary>
        /// Make sure types are fully qualified by namespace in the case that there are
        /// multiple classes with the same name.
        /// </summary>
        [Fact]
        public void NoDiagnostics_SameClassName()
        {
            var hostPropertyTypeInfo = new EmptyClassBuilder()
                .WithClassName("ClassName")
                .WithNamespace("Namespace1")
                .WithClassAccess(Accessibility.Public);
            var hostTypeInfo = new WhenChangedHostBuilder()
                .WithClassName("ClassName")
                .WithNamespace("Namespace2")
                .WithInvocation(InvocationKind.MemberAccess, ReceiverKind.This, x => x.Child, x => x.Value, (a, b) => "result" + a + b)
                .WithClassAccess(Accessibility.Public)
                .WithPropertyType(hostPropertyTypeInfo)
                .WithPropertyAccess(Accessibility.Public);

            var fixture = WhenChangedFixture.Create(hostTypeInfo, _testOutputHelper, hostPropertyTypeInfo.BuildRoot());
            fixture.RunGenerator(out var compilationDiagnostics, out var generatorDiagnostics, useRoslyn: true, saveCompilation: false);

            Assert.Empty(generatorDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));
            Assert.Empty(compilationDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));

            string Convert(WhenChangedHostProxy host)
            {
                return "result" + host.Child + host.Value;
            }

            var host = fixture.NewHostInstance();
            host.Value = fixture.NewValuePropertyInstance();
            var observable = host.GetWhenChangedObservable(_ => _testOutputHelper.WriteLine(fixture.Sources));
            object value = null;
            observable.Subscribe(x => value = x);
            Assert.Equal(Convert(host), value);
            host.Value = fixture.NewValuePropertyInstance();
            Assert.Equal(Convert(host), value);
            host.Value = null;
            Assert.Equal(Convert(host), value);
        }

        /// <summary>
        /// Make sure chains of length 2 are handled correctly.
        /// </summary>
        [Fact]
        public void NoDiagnostics_ChainOf2()
        {
            var hostPropertyTypeInfo = new EmptyClassBuilder()
                .WithClassName("ClassName")
                .WithNamespace("Namespace1")
                .WithClassAccess(Accessibility.Public);
            var hostTypeInfo = new WhenChangedHostBuilder()
                .WithClassName("ClassName")
                .WithNamespace("Namespace2")
                .WithInvocation(InvocationKind.MemberAccess, ReceiverKind.This, x => x.Child.Value)
                .WithClassAccess(Accessibility.Public)
                .WithPropertyType(hostPropertyTypeInfo)
                .WithPropertyAccess(Accessibility.Public);

            var fixture = WhenChangedFixture.Create(hostTypeInfo, _testOutputHelper, hostPropertyTypeInfo.BuildRoot());
            fixture.RunGenerator(out var compilationDiagnostics, out var generatorDiagnostics, useRoslyn: true, saveCompilation: false);

            Assert.Empty(generatorDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));
            Assert.Empty(compilationDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));

            var host = fixture.NewHostInstance();
            host.Child = fixture.NewHostInstance();
            host.Child.Value = fixture.NewValuePropertyInstance();
            var observable = host.GetWhenChangedObservable(_ => _testOutputHelper.WriteLine(fixture.Sources));
            object value = null;
            observable.Subscribe(x => value = x);
            Assert.Equal(host.Child.Value, value);

            // According to the current design, no emission should occur when the chain is "broken".
            host.Child = null;
            Assert.NotNull(value);

            host.Child = fixture.NewHostInstance();
            Assert.Equal(host.Child.Value, value);
            host.Child.Value = fixture.NewValuePropertyInstance();
            Assert.Equal(host.Child.Value, value);
        }

        /// <summary>
        /// Make sure chains of length 3 are handled correctly.
        /// </summary>
        [Fact]
        public void NoDiagnostics_ChainOf3()
        {
            var hostPropertyTypeInfo = new EmptyClassBuilder()
                .WithClassName("ClassName")
                .WithNamespace("Namespace1")
                .WithClassAccess(Accessibility.Public);
            var hostTypeInfo = new WhenChangedHostBuilder()
                .WithClassName("ClassName")
                .WithNamespace("Namespace2")
                .WithInvocation(InvocationKind.MemberAccess, ReceiverKind.This, x => x.Child.Child.Value)
                .WithClassAccess(Accessibility.Public)
                .WithPropertyType(hostPropertyTypeInfo)
                .WithPropertyAccess(Accessibility.Public);

            var fixture = WhenChangedFixture.Create(hostTypeInfo, _testOutputHelper, hostPropertyTypeInfo.BuildRoot());
            fixture.RunGenerator(out var compilationDiagnostics, out var generatorDiagnostics, useRoslyn: true, saveCompilation: false);

            Assert.Empty(generatorDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));
            Assert.Empty(compilationDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));

            var host = fixture.NewHostInstance();
            host.Child = fixture.NewHostInstance();
            host.Child.Child = fixture.NewHostInstance();
            host.Child.Child.Value = fixture.NewValuePropertyInstance();
            var observable = host.GetWhenChangedObservable(_ => _testOutputHelper.WriteLine(fixture.Sources));
            object value = null;
            observable.Subscribe(x => value = x);
            Assert.Equal(host.Child.Child.Value, value);

            // According to the current design, no emission should occur when the chain is "broken".
            host.Child = null;
            Assert.NotNull(value);

            host.Child = fixture.NewHostInstance();
            host.Child.Child = fixture.NewHostInstance();
            Assert.Equal(host.Child.Value, value);
            host.Child.Child.Value = fixture.NewValuePropertyInstance();
            Assert.Equal(host.Child.Child.Value, value);
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
        public void NoDiagnostics_AccessModifierCombinations_Roslyn(Accessibility hostContainerTypeAccess, Accessibility hostTypeAccess, Accessibility propertyTypeAccess, Accessibility propertyAccess)
        {
            NoDiagnostics_AccessModifierCombinations(hostContainerTypeAccess, hostTypeAccess, propertyTypeAccess, propertyAccess, true);
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
        public void NoDiagnostics_AccessModifierCombinations_StringBuilder(Accessibility hostContainerTypeAccess, Accessibility hostTypeAccess, Accessibility propertyTypeAccess, Accessibility propertyAccess)
        {
            NoDiagnostics_AccessModifierCombinations(hostContainerTypeAccess, hostTypeAccess, propertyTypeAccess, propertyAccess, false);
        }

        private void NoDiagnostics_AccessModifierCombinations(Accessibility hostContainerTypeAccess, Accessibility hostTypeAccess, Accessibility propertyTypeAccess, Accessibility propertyAccess, bool useRoslyn)
        {
            var hostPropertyTypeInfo = new EmptyClassBuilder()
                .WithClassAccess(propertyTypeAccess);
            var hostTypeInfo = new WhenChangedHostBuilder()
                .WithClassName("Host")
                .WithInvocation(InvocationKind.MemberAccess, ReceiverKind.This, x => x.Child, x => x.Value, (a, b) => "result" + a + b)
                .WithClassAccess(hostTypeAccess)
                .AddNestedClass(hostPropertyTypeInfo)
                .WithPropertyType(hostPropertyTypeInfo)
                .WithPropertyAccess(propertyAccess);
            var hostContainerTypeInfo = new EmptyClassBuilder()
                .WithClassName("HostContainer")
                .WithClassAccess(hostContainerTypeAccess)
                .AddNestedClass(hostTypeInfo);

            var fixture = WhenChangedFixture.Create(hostTypeInfo, _testOutputHelper);
            fixture.RunGenerator(out var compilationDiagnostics, out var generatorDiagnostics, useRoslyn, saveCompilation: true);

            Assert.Empty(generatorDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));
            Assert.Empty(compilationDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));

            string Convert(WhenChangedHostProxy host)
            {
                return "result" + host.Child + host.Value;
            }

            var host = fixture.NewHostInstance();
            host.Value = fixture.NewValuePropertyInstance();
            var observable = host.GetWhenChangedObservable(_ => _testOutputHelper.WriteLine(fixture.Sources));
            object value = null;
            observable.Subscribe(x => value = x);
            Assert.Equal(Convert(host), value);
            host.Value = fixture.NewValuePropertyInstance();
            Assert.Equal(Convert(host), value);
            host.Value = null;
            Assert.Equal(Convert(host), value);
        }
    }
}
