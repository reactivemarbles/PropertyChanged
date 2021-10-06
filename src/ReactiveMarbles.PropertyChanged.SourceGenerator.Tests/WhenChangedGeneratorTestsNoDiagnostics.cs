// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReactiveMarbles.PropertyChanged.SourceGenerator.Builders;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Tests;

/// <summary>
/// WhenChanged generator tests.
/// </summary>
[TestClass]
public class WhenChangedGeneratorTestsNoDiagnostics
{
    private readonly CompilationUtil _compilationUtil;

    /// <summary>
    /// Initializes a new instance of the <see cref="WhenChangedGeneratorTestsNoDiagnostics"/> class.
    /// </summary>
    public WhenChangedGeneratorTestsNoDiagnostics() => _compilationUtil = new(x => TestContext?.WriteLine(x));

    /// <summary>
    /// Gets or sets the test context.
    /// </summary>
    public TestContext TestContext { get; set; }

    /// <summary>
    /// Initializes the class test.
    /// </summary>
    /// <returns>A task.</returns>
    [TestInitialize]
    public Task InitializeAsync() => _compilationUtil.Initialize();

    /// <summary>
    /// Make sure the ReceiverKind.Instance is handled correctly.
    /// </summary>
    /// <param name="invocationKind">The kind of invocation.</param>
    /// <returns>A task to monitor the progress.</returns>
    [DataTestMethod]
    [DataRow(InvocationKind.MemberAccess)]
    [DataRow(InvocationKind.Explicit)]
    public async Task NoDiagnostics_InstanceReceiverKind(InvocationKind invocationKind)
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
            .WithClassName("HostClass")
            .WithClassAccess(Accessibility.Public)
            .WithInvocation(invocationKind, x => x.Value, externalReceiverTypeInfo);

        var fixture = await WhenChangedFixture.Create(hostTypeInfo, externalReceiverTypeInfo, x => TestContext.WriteLine(x), ("ReceiverPropertyTypeInfo.cs", receiverPropertyTypeInfo.BuildRoot())).ConfigureAwait(false);
        fixture.RunGenerator(out var compilationDiagnostics, out var generatorDiagnostics);

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
    /// <returns>A task to monitor the progress.</returns>
    [DataTestMethod]
    [DataRow(Accessibility.Public, Accessibility.Public, Accessibility.Public)]
    [DataRow(Accessibility.Public, Accessibility.Public, Accessibility.Internal)]
    [DataRow(Accessibility.Public, Accessibility.Internal, Accessibility.Internal)]
    [DataRow(Accessibility.Internal, Accessibility.Public, Accessibility.Public)]
    [DataRow(Accessibility.Internal, Accessibility.Public, Accessibility.Internal)]
    [DataRow(Accessibility.Internal, Accessibility.Internal, Accessibility.Public)]
    [DataRow(Accessibility.Internal, Accessibility.Internal, Accessibility.Internal)]
    public Task NoDiagnostics_ExplicitInvocation_MultiExpression(Accessibility hostTypeAccess, Accessibility propertyTypeAccess, Accessibility propertyAccess)
    {
        var hostPropertyTypeInfo = new EmptyClassBuilder()
            .WithClassAccess(propertyTypeAccess);
        var hostTypeInfo = new WhenChangedHostBuilder()
            .WithClassName("HostClass")
            .WithClassAccess(hostTypeAccess)
            .WithPropertyType(hostPropertyTypeInfo)
            .WithPropertyAccess(propertyAccess);

        return AssertTestCase_MultiExpression(hostTypeInfo, InvocationKind.Explicit, ("HostPropertyTypeInfo.cs", hostPropertyTypeInfo.BuildRoot()));
    }

    /// <summary>
    /// Make sure namespaces are handled correctly.
    /// </summary>
    /// <param name="hostTypeNamespace">The namespace containing the host.</param>
    /// <param name="hostPropertyTypeNamespace">The namespace containing the property type.</param>
    /// <returns>A task to monitor the progress.</returns>
    [DataTestMethod]
    [DataRow(null, null)]
    [DataRow("HostNamespace", "CustomNamespace")]
    [DataRow("HostNamespace", null)]
    [DataRow(null, "CustomNamespace")]
    public Task NoDiagnostics_Namespaces(string hostTypeNamespace, string hostPropertyTypeNamespace)
    {
        var hostPropertyTypeInfo = new EmptyClassBuilder()
            .WithClassAccess(Accessibility.Public)
            .WithNamespace(hostPropertyTypeNamespace);
        var hostTypeInfo = new WhenChangedHostBuilder()
            .WithClassName("HostClass")
            .WithClassAccess(Accessibility.Public)
            .WithPropertyType(hostPropertyTypeInfo)
            .WithPropertyAccess(Accessibility.Public)
            .WithNamespace(hostTypeNamespace);

        return AssertTestCase_MultiExpression(hostTypeInfo, InvocationKind.MemberAccess, ("HostPropertyTypeInfo.cs", hostPropertyTypeInfo.BuildRoot()));
    }

    /// <summary>
    /// Make sure types are fully qualified by namespace in the case that there are
    /// multiple classes with the same name.
    /// </summary>
    /// <returns>A task to monitor the progress.</returns>
    [TestMethod]
    public Task NoDiagnostics_SameClassName()
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

        return AssertTestCase_MultiExpression(hostTypeInfo, InvocationKind.MemberAccess, ("HostPropertyTypeInfo.cs", hostPropertyTypeInfo.BuildRoot()));
    }

    /// <summary>
    /// Make sure chains of length 2 are handled correctly.
    /// </summary>
    /// <returns>A task to monitor the progress.</returns>
    [TestMethod]
    public async Task NoDiagnostics_ChainOf2()
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

        var fixture = await WhenChangedFixture.Create(hostTypeInfo, x => TestContext.WriteLine(x), ("HostPropertyTypeInfo.cs", hostPropertyTypeInfo.BuildRoot())).ConfigureAwait(false);
        fixture.RunGenerator(out var compilationDiagnostics, out var generatorDiagnostics);

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
    /// <returns>A task to monitor the progress.</returns>
    [TestMethod]
    public async Task NoDiagnostics_ChainOf3()
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

        var fixture = await WhenChangedFixture.Create(hostTypeInfo, x => TestContext.WriteLine(x), ("HostPropertyTypeInfo.cs", hostPropertyTypeInfo.BuildRoot())).ConfigureAwait(false);
        fixture.RunGenerator(out var compilationDiagnostics, out var generatorDiagnostics);

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
    /// <returns>A task to monitor the progress.</returns>
    [DataTestMethod]
    [DynamicData(nameof(AccessibilityTestCases.GetValidAccessModifierCombinations), typeof(AccessibilityTestCases), DynamicDataSourceType.Method)]
    public Task NoDiagnostics_AccessModifierCombinations(Accessibility hostContainerTypeAccess, Accessibility hostTypeAccess, Accessibility propertyTypeAccess, Accessibility propertyAccess)
    {
        var hostPropertyTypeInfo = new EmptyClassBuilder()
            .WithClassAccess(propertyTypeAccess);
        var hostTypeInfo = new WhenChangedHostBuilder()
            .WithClassName("HostClass")
            .WithClassAccess(hostTypeAccess)
            .AddNestedClass(hostPropertyTypeInfo)
            .WithPropertyType(hostPropertyTypeInfo)
            .WithPropertyAccess(propertyAccess);
        var hostContainerTypeInfo = new EmptyClassBuilder()
            .WithClassName("HostContainer")
            .WithClassAccess(hostContainerTypeAccess)
            .AddNestedClass(hostTypeInfo);

        return AssertTestCase_MultiExpression(hostTypeInfo, InvocationKind.MemberAccess);
    }

    /// <summary>
    /// Make sure two custom types are handled correctly..
    /// </summary>
    /// <returns>A task to monitor the progress.</returns>
    [TestMethod]
    public async Task NoDiagnostics_PrivateHostPropertyTypeAndInternalOutputType()
    {
        var hostPropertyTypeInfo = new EmptyClassBuilder()
            .WithClassAccess(Accessibility.Private);
        var customType = new EmptyClassBuilder()
            .WithClassName("Output")
            .WithClassAccess(Accessibility.Internal);
        var hostTypeInfo = new WhenChangedHostBuilder()
            .WithClassName("HostClass")
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

        var fixture = await WhenChangedFixture.Create(hostTypeInfo, x => TestContext.WriteLine(x)).ConfigureAwait(false);
        fixture.RunGenerator(out var compilationDiagnostics, out var generatorDiagnostics);

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

    private async Task AssertTestCase_SingleExpression(WhenChangedHostBuilder hostTypeInfo, BaseUserSourceBuilder hostPropertyTypeInfo, bool typesHaveSameRoot)
    {
        hostTypeInfo.WithInvocation(InvocationKind.MemberAccess, x => x.Value);
        var propertyTypeSource = typesHaveSameRoot ? string.Empty : hostPropertyTypeInfo.BuildRoot();
        var fixture = await WhenChangedFixture.Create(hostTypeInfo, x => TestContext.WriteLine(x), ("PropertyTypeSource.cs", propertyTypeSource)).ConfigureAwait(false);
        fixture.RunGenerator(out var compilationDiagnostics, out var generatorDiagnostics);

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

    private async Task AssertTestCase_MultiExpression(WhenChangedHostBuilder hostTypeInfo, InvocationKind invocationKind, params (string FileName, string Source)[] extraSources)
    {
        hostTypeInfo.WithInvocation(invocationKind, x => x.Child, x => x.Value, (a, b) => "result" + a + b);
        var fixture = await WhenChangedFixture.Create(hostTypeInfo, x => TestContext.WriteLine(x), extraSources).ConfigureAwait(false);
        fixture.RunGenerator(out var compilationDiagnostics, out var generatorDiagnostics);

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
