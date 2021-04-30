// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Linq;

using FluentAssertions;

using Microsoft.CodeAnalysis;

using ReactiveMarbles.PropertyChanged.SourceGenerator.Builders;

using Xunit;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Tests
{
    /// <summary>
    /// WhenChanged generator tests.
    /// </summary>
    public partial class WhenChangedGeneratorTests
    {
        /// <summary>
        /// Make sure the ReceiverKind.Instance is handled correctly.
        /// </summary>
        /// <param name="invocationKind">The kind of invocation.</param>
        [Theory]
        [InlineData(InvocationKind.MemberAccess)]
        [InlineData(InvocationKind.Explicit)]
        public void NoDiagnostics_InstanceReceiverKind(InvocationKind invocationKind)
        {
            var receiverPropertyTypeInfo = new EmptyClassBuilder()
                .WithClassAccess(Accessibility.Public);
            var externalReceiverTypeInfo = new WhenChangedHostBuilder()
                .WithClassName("ReactiveType")
                .WithClassAccess(Accessibility.Public)
                .WithPropertyType(receiverPropertyTypeInfo)
                .WithPropertyAccess(Accessibility.Public)
                .WithInvocation("null");
            var hostTypeInfo = new WhenChangedHostBuilder()
                .WithClassName("Host")
                .WithClassAccess(Accessibility.Public)
                .WithInvocation(invocationKind, x => x.Value, externalReceiverTypeInfo);

            var fixture = WhenChangedFixture.Create(hostTypeInfo, externalReceiverTypeInfo, TestContext, receiverPropertyTypeInfo.BuildRoot());
            fixture.RunGenerator(out var compilationDiagnostics, out var generatorDiagnostics, saveCompilation: false);

            generatorDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning).Should().BeEmpty();
            compilationDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning).Should().BeEmpty();

            var host = fixture.NewHostInstance();
            var receiver = fixture.NewReceiverInstance();
            host.Receiver = receiver;
            receiver.Value = fixture.NewValuePropertyInstance();
            var observable = host.GetWhenChangedObservable(_ => TestContext.WriteLine(fixture.Sources));
            object value = null;
            observable.Subscribe(x => value = x);
            receiver.Value.Should().Equals(value);
            receiver.Value = fixture.NewValuePropertyInstance();
            receiver.Value.Should().Equals(value);
            receiver.Value = null;
            receiver.Value.Should().Equals(value);
        }

        /// <summary>
        /// Make sure explicit invocations are handled correctly for visible types and properties.
        /// </summary>
        /// <param name="hostTypeAccess">The access modifier of the host type.</param>
        /// <param name="propertyTypeAccess">The access modifier of the Value property type on the host.</param>
        /// <param name="propertyAccess">The access modifier of the host properties.</param>
        [Theory]
        [InlineData(Accessibility.Public, Accessibility.Public, Accessibility.Public)]
        [InlineData(Accessibility.Public, Accessibility.Public, Accessibility.Internal)]
        [InlineData(Accessibility.Public, Accessibility.Internal, Accessibility.Internal)]
        [InlineData(Accessibility.Internal, Accessibility.Public, Accessibility.Public)]
        [InlineData(Accessibility.Internal, Accessibility.Public, Accessibility.Internal)]
        [InlineData(Accessibility.Internal, Accessibility.Internal, Accessibility.Public)]
        [InlineData(Accessibility.Internal, Accessibility.Internal, Accessibility.Internal)]
        public void NoDiagnostics_ExplicitInvocation_MultiExpression(Accessibility hostTypeAccess, Accessibility propertyTypeAccess, Accessibility propertyAccess)
        {
            var hostPropertyTypeInfo = new EmptyClassBuilder()
                .WithClassAccess(propertyTypeAccess);
            var hostTypeInfo = new WhenChangedHostBuilder()
                .WithClassName("Host")
                .WithClassAccess(hostTypeAccess)
                .WithPropertyType(hostPropertyTypeInfo)
                .WithPropertyAccess(propertyAccess);

            AssertTestCase_MultiExpression(hostTypeInfo, hostPropertyTypeInfo.BuildRoot());
        }

        /// <summary>
        /// Make sure namespaces are handled correctly.
        /// </summary>
        /// <param name="hostTypeNamespace">The namespace containing the host.</param>
        /// <param name="hostPropertyTypeNamespace">The namespace containing the property type.</param>
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
                .WithClassAccess(Accessibility.Public)
                .WithPropertyType(hostPropertyTypeInfo)
                .WithPropertyAccess(Accessibility.Public)
                .WithNamespace(hostTypeNamespace);

            AssertTestCase_MultiExpression(hostTypeInfo, hostPropertyTypeInfo.BuildRoot());
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
                .WithClassAccess(Accessibility.Public)
                .WithPropertyType(hostPropertyTypeInfo)
                .WithPropertyAccess(Accessibility.Public);

            AssertTestCase_MultiExpression(hostTypeInfo, hostPropertyTypeInfo.BuildRoot());
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
                .WithInvocation(InvocationKind.MemberAccess, x => x.Child.Value)
                .WithClassAccess(Accessibility.Public)
                .WithPropertyType(hostPropertyTypeInfo)
                .WithPropertyAccess(Accessibility.Public);

            var fixture = WhenChangedFixture.Create(hostTypeInfo, TestContext, hostPropertyTypeInfo.BuildRoot());
            fixture.RunGenerator(out var compilationDiagnostics, out var generatorDiagnostics, saveCompilation: false);

            generatorDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning).Should().BeEmpty();
            compilationDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning).Should().BeEmpty();

            var host = fixture.NewHostInstance();
            host.Child = fixture.NewHostInstance();
            host.Child.Value = fixture.NewValuePropertyInstance();
            var observable = host.GetWhenChangedObservable(_ => TestContext.WriteLine(fixture.Sources));
            object value = null;
            observable.Subscribe(x => value = x);
            host.Child.Value.Should().Be(value);

            // According to the current design, no emission should occur when the chain is "broken".
            host.Child = null;
            value.Should().NotBeNull();

            host.Child = fixture.NewHostInstance();
            host.Child.Value.Should().Be(value);
            host.Child.Value = fixture.NewValuePropertyInstance();
            host.Child.Value.Should().Be(value);
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
                .WithInvocation(InvocationKind.MemberAccess, x => x.Child.Child.Value)
                .WithClassAccess(Accessibility.Public)
                .WithPropertyType(hostPropertyTypeInfo)
                .WithPropertyAccess(Accessibility.Public);

            var fixture = WhenChangedFixture.Create(hostTypeInfo, TestContext, hostPropertyTypeInfo.BuildRoot());
            fixture.RunGenerator(out var compilationDiagnostics, out var generatorDiagnostics, saveCompilation: false);

            generatorDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning).Should().BeEmpty();
            compilationDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning).Should().BeEmpty();

            var host = fixture.NewHostInstance();
            host.Child = fixture.NewHostInstance();
            host.Child.Child = fixture.NewHostInstance();
            host.Child.Child.Value = fixture.NewValuePropertyInstance();
            var observable = host.GetWhenChangedObservable(_ => TestContext.WriteLine(fixture.Sources));
            object value = null;
            observable.Subscribe(x => value = x);
            host.Child.Child.Value.Should().Be(value);

            // According to the current design, no emission should occur when the chain is "broken".
            host.Child = null;
            value.Should().NotBeNull();

            host.Child = fixture.NewHostInstance();
            host.Child.Child = fixture.NewHostInstance();
            host.Child.Value.Should().Be(value);
            host.Child.Child.Value = fixture.NewValuePropertyInstance();
            host.Child.Child.Value.Should().Be(value);
        }

        /// <summary>
        /// Make sure all possible combinations of access modifiers result in successful generation.
        /// </summary>
        /// <param name="hostContainerTypeAccess">The access modifier of the type containing the host type.</param>
        /// <param name="hostTypeAccess">The access modifier of the host type.</param>
        /// <param name="propertyTypeAccess">The access modifier of the Value property type on the host.</param>
        /// <param name="propertyAccess">The access modifier of the host properties.</param>
        [Theory]
        [MemberData(nameof(AccessibilityTestCases.GetValidAccessModifierCombinations), MemberType = typeof(AccessibilityTestCases))]
        public void NoDiagnostics_AccessModifierCombinations(Accessibility hostContainerTypeAccess, Accessibility hostTypeAccess, Accessibility propertyTypeAccess, Accessibility propertyAccess)
        {
            var hostPropertyTypeInfo = new EmptyClassBuilder()
                .WithClassAccess(propertyTypeAccess);
            var hostTypeInfo = new WhenChangedHostBuilder()
                .WithClassName("Host")
                .WithClassAccess(hostTypeAccess)
                .AddNestedClass(hostPropertyTypeInfo)
                .WithPropertyType(hostPropertyTypeInfo)
                .WithPropertyAccess(propertyAccess);
            var hostContainerTypeInfo = new EmptyClassBuilder()
                .WithClassName("HostContainer")
                .WithClassAccess(hostContainerTypeAccess)
                .AddNestedClass(hostTypeInfo);

            AssertTestCase_MultiExpression(hostTypeInfo);
        }

        /// <summary>
        /// Make sure two custom types are handled correctly..
        /// </summary>
        [Fact]
        public void NoDiagnostics_PrivateHostPropertyTypeAndInternalOutputType()
        {
            var hostPropertyTypeInfo = new EmptyClassBuilder()
                .WithClassAccess(Accessibility.Private);
            var customType = new EmptyClassBuilder()
                .WithClassName("Output")
                .WithClassAccess(Accessibility.Internal);
            var hostTypeInfo = new WhenChangedHostBuilder()
                .WithClassName("Host")
                .WithInvocation(InvocationKind.MemberAccess, x => x.Child, x => x.Value, "(a, b) => b != null ? new HostContainer.Output() : null")
                .WithClassAccess(Accessibility.Protected)
                .AddNestedClass(hostPropertyTypeInfo)
                .WithPropertyType(hostPropertyTypeInfo)
                .WithPropertyAccess(Accessibility.Private);
            var hostContainerTypeInfo = new EmptyClassBuilder()
                .WithClassName("HostContainer")
                .WithClassAccess(Accessibility.Public)
                .AddNestedClass(hostTypeInfo)
                .AddNestedClass(customType);

            var fixture = WhenChangedFixture.Create(hostTypeInfo, TestContext);
            fixture.RunGenerator(out var compilationDiagnostics, out var generatorDiagnostics, saveCompilation: false);

            generatorDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning).Should().BeEmpty();
            compilationDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning).Should().BeEmpty();

            var host = fixture.NewHostInstance();
            host.Value = fixture.NewValuePropertyInstance();
            var observable = host.GetWhenChangedObservable(_ => TestContext.WriteLine(fixture.Sources));
            object value = null;
            observable.Subscribe(x => value = x);

            // TODO: Better series of checks. Currently can't compare values because of reference
            // equality and we don't have access to the instance that the conversionFunc creates.
            value.Should().NotBeNull();
            host.Value = null;
            value.Should().BeNull();
            host.Value = fixture.NewValuePropertyInstance();
            value.Should().NotBeNull();
        }

        private void AssertTestCase_SingleExpression(WhenChangedHostBuilder hostTypeInfo, BaseUserSourceBuilder hostPropertyTypeInfo, bool typesHaveSameRoot)
        {
            hostTypeInfo.WithInvocation(InvocationKind.MemberAccess, x => x.Value);
            var propertyTypeSource = typesHaveSameRoot ? string.Empty : hostPropertyTypeInfo.BuildRoot();
            var fixture = WhenChangedFixture.Create(hostTypeInfo, TestContext, propertyTypeSource);
            fixture.RunGenerator(out var compilationDiagnostics, out var generatorDiagnostics, saveCompilation: false);

            generatorDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning).Should().BeEmpty();
            compilationDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning).Should().BeEmpty();

            var host = fixture.NewHostInstance();
            host.Value = fixture.NewValuePropertyInstance();
            var observable = host.GetWhenChangedObservable(_ => TestContext.WriteLine(fixture.Sources));
            object value = null;
            observable.Subscribe(x => value = x);
            host.Value.Should().Be(value);
            host.Value = fixture.NewValuePropertyInstance();
            host.Value.Should().Be(value);
            host.Value = null;
            host.Value.Should().Be(value);
        }

        private void AssertTestCase_MultiExpression(WhenChangedHostBuilder hostTypeInfo, params string[] extraSources)
        {
            hostTypeInfo.WithInvocation(InvocationKind.MemberAccess, x => x.Child, x => x.Value, (a, b) => "result" + a + b);
            var fixture = WhenChangedFixture.Create(hostTypeInfo, TestContext, extraSources);
            fixture.RunGenerator(out var compilationDiagnostics, out var generatorDiagnostics, saveCompilation: false);

            generatorDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning).Should().BeEmpty();
            compilationDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning).Should().BeEmpty();

            static string Convert(WhenChangedHostProxy host) => "result" + host.Child + host.Value;

            var host = fixture.NewHostInstance();
            host.Value = fixture.NewValuePropertyInstance();
            var observable = host.GetWhenChangedObservable(_ => TestContext.WriteLine(fixture.Sources));
            object value = null;
            observable.Subscribe(x => value = x);
            Convert(host).Should().Be(value.ToString());
            host.Value = fixture.NewValuePropertyInstance();
            Convert(host).Should().Be(value.ToString());
            host.Value = null;
            Convert(host).Should().Be(value.ToString());
        }
    }
}
