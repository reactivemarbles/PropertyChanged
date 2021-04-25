// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Linq;
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
            var hostPropertyTypeInfo = new EmptyClassBuilder()
                .WithClassAccess(Accessibility.Public);
            var hostTypeInfo = new WhenChangedHostBuilder()
                .WithClassName("Host")
                .WithInvocation(invocationKind, ReceiverKind.Instance, x => x.Value)
                .WithClassAccess(Accessibility.Public)
                .WithPropertyType(hostPropertyTypeInfo)
                .WithPropertyAccess(Accessibility.Public);

            AssertTestCase_MultiExpression(hostTypeInfo, hostPropertyTypeInfo, typesHaveSameRoot: false);
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
                .WithInvocation(InvocationKind.Explicit, ReceiverKind.This, x => x.Child, x => x.Value, (a, b) => "result" + a + b)
                .WithClassAccess(hostTypeAccess)
                .WithPropertyType(hostPropertyTypeInfo)
                .WithPropertyAccess(propertyAccess);

            AssertTestCase_MultiExpression(hostTypeInfo, hostPropertyTypeInfo, typesHaveSameRoot: false);
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
                .WithInvocation(InvocationKind.MemberAccess, ReceiverKind.This, x => x.Child, x => x.Value, (a, b) => "result" + a + b)
                .WithClassAccess(Accessibility.Public)
                .WithPropertyType(hostPropertyTypeInfo)
                .WithPropertyAccess(Accessibility.Public)
                .WithNamespace(hostTypeNamespace);

            AssertTestCase_MultiExpression(hostTypeInfo, hostPropertyTypeInfo, typesHaveSameRoot: false);
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

            AssertTestCase_MultiExpression(hostTypeInfo, hostPropertyTypeInfo, typesHaveSameRoot: false);
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

            var fixture = WhenChangedFixture.Create(hostTypeInfo, _testLogger, hostPropertyTypeInfo.BuildRoot());
            fixture.RunGenerator(out var compilationDiagnostics, out var generatorDiagnostics, saveCompilation: false);

            Assert.Empty(generatorDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));
            Assert.Empty(compilationDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));

            var host = fixture.NewHostInstance();
            host.Child = fixture.NewHostInstance();
            host.Child.Value = fixture.NewValuePropertyInstance();
            var observable = host.GetWhenChangedObservable(_ => _testLogger.WriteLine(fixture.Sources));
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

            var fixture = WhenChangedFixture.Create(hostTypeInfo, _testLogger, hostPropertyTypeInfo.BuildRoot());
            fixture.RunGenerator(out var compilationDiagnostics, out var generatorDiagnostics, saveCompilation: false);

            Assert.Empty(generatorDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));
            Assert.Empty(compilationDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));

            var host = fixture.NewHostInstance();
            host.Child = fixture.NewHostInstance();
            host.Child.Child = fixture.NewHostInstance();
            host.Child.Child.Value = fixture.NewValuePropertyInstance();
            var observable = host.GetWhenChangedObservable(_ => _testLogger.WriteLine(fixture.Sources));
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
                .WithInvocation(InvocationKind.MemberAccess, ReceiverKind.This, x => x.Child, x => x.Value, (a, b) => "result" + a + b)
                .WithClassAccess(hostTypeAccess)
                .AddNestedClass(hostPropertyTypeInfo)
                .WithPropertyType(hostPropertyTypeInfo)
                .WithPropertyAccess(propertyAccess);
            var hostContainerTypeInfo = new EmptyClassBuilder()
                .WithClassName("HostContainer")
                .WithClassAccess(hostContainerTypeAccess)
                .AddNestedClass(hostTypeInfo);

            AssertTestCase_MultiExpression(hostTypeInfo, hostPropertyTypeInfo, typesHaveSameRoot: true);
        }

        private void AssertTestCase_SingleExpression(WhenChangedHostBuilder hostTypeInfo, BaseUserSourceBuilder hostPropertyTypeInfo, bool typesHaveSameRoot)
        {
            hostTypeInfo.WithInvocation(InvocationKind.MemberAccess, ReceiverKind.This, x => x.Value);
            var propertyTypeSource = typesHaveSameRoot ? string.Empty : hostPropertyTypeInfo.BuildRoot();
            var fixture = WhenChangedFixture.Create(hostTypeInfo, _testLogger, propertyTypeSource);
            fixture.RunGenerator(out var compilationDiagnostics, out var generatorDiagnostics, saveCompilation: false);

            Assert.Empty(generatorDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));
            Assert.Empty(compilationDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));

            var host = fixture.NewHostInstance();
            host.Value = fixture.NewValuePropertyInstance();
            var observable = host.GetWhenChangedObservable(_ => _testLogger.WriteLine(fixture.Sources));
            object value = null;
            observable.Subscribe(x => value = x);
            Assert.Equal(host.Value, value);
            host.Value = fixture.NewValuePropertyInstance();
            Assert.Equal(host.Value, value);
            host.Value = null;
            Assert.Equal(host.Value, value);
        }

        private void AssertTestCase_MultiExpression(WhenChangedHostBuilder hostTypeInfo, BaseUserSourceBuilder hostPropertyTypeInfo, bool typesHaveSameRoot)
        {
            hostTypeInfo.WithInvocation(InvocationKind.MemberAccess, ReceiverKind.This, x => x.Child, x => x.Value, (a, b) => "result" + a + b);
            var propertyTypeSource = typesHaveSameRoot ? string.Empty : hostPropertyTypeInfo.BuildRoot();
            var fixture = WhenChangedFixture.Create(hostTypeInfo, _testLogger, propertyTypeSource);
            fixture.RunGenerator(out var compilationDiagnostics, out var generatorDiagnostics, saveCompilation: false);

            Assert.Empty(generatorDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));
            Assert.Empty(compilationDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning));

            static string Convert(WhenChangedHostProxy host)
            {
                return "result" + host.Child + host.Value;
            }

            var host = fixture.NewHostInstance();
            host.Value = fixture.NewValuePropertyInstance();
            var observable = host.GetWhenChangedObservable(_ => _testLogger.WriteLine(fixture.Sources));
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
