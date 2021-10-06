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
/// WhenChanging generator tests.
/// </summary>
public class WhenChangingGeneratorTestsDiagnostics
{
    private const string WhenChangingPlaceholder = "[whenchanging_invocation]";
    private const string SourceTemplate = @"
using System;
using System.ComponentModel;
using System.Linq.Expressions;

public class HostClass : INotifyPropertyChanging
{
    public HostClass()
    {
        [whenchanging_invocation];
    }

    public event PropertyChangingEventHandler PropertyChanging;
    public HostClass Child { get; }
    public string Value { get; }
    public HostClass[] Children { get; }
    public string[] Values { get; }
    public Expression<Func<HostClass, string>> MyExpression => x => x.Value;
    public Expression<Func<HostClass, string>> GetMyExpression() => x => x.Value;
    public HostClass GetChild() => Child;
    public string GetValue() => Value;
}
";

    private readonly CompilationUtil _compilationUtil;

    /// <summary>
    /// Initializes a new instance of the <see cref="WhenChangingGeneratorTestsDiagnostics"/> class.
    /// </summary>
    public WhenChangingGeneratorTestsDiagnostics() => _compilationUtil = new(x => TestContext?.WriteLine(x));

    /// <summary>
    /// Gets or sets the test context.
    /// </summary>
    public TestContext TestContext { get; set;  }

    /// <summary>
    /// Initializes the class.
    /// </summary>
    /// <returns>A task.</returns>
    [TestInitialize]
    public Task InitializeAsync() => _compilationUtil.Initialize();

    /// <summary>
    /// Expression arguments may not be specified as a property pointing to the actual expression.
    /// Yes: this.WhenChanging(x => x.Value).
    /// No: this.WhenChanging(MyExpression).
    /// </summary>
    [TestMethod]
    public void RXM001_PropertyInvocationUsedAsAnExpressionArgument()
    {
        const string invocation = "this.WhenChanging(MyExpression)";
        var source = SourceTemplate.Replace(WhenChangingPlaceholder, invocation);
        AssertDiagnostic(source, DiagnosticWarnings.ExpressionMustBeInline);
    }

    /// <summary>
    /// Expression arguments may not be specified as a property pointing to the actual expression.
    /// Yes: this.WhenChanging(x => x.Value).
    /// No: this.WhenChanging(MyExpression).
    /// </summary>
    [TestMethod]
    public void RXM001_PropertyInvocationUsedAsAnExpressionArgument_MultiExpressionMultiChain()
    {
        const string invocation = "this.WhenChanging(x => x.Child.Value, MyExpression, (a, b) => $\"{a}-{b}\")";
        var source = SourceTemplate.Replace(WhenChangingPlaceholder, invocation);
        AssertDiagnostic(source, DiagnosticWarnings.ExpressionMustBeInline);
    }

    /// <summary>
    /// Expression arguments may not be specified as a method pointing to the actual expression.
    /// Yes: this.WhenChanging(x => x.Value).
    /// No: this.WhenChanging(GetMyExpression()).
    /// </summary>
    [TestMethod]
    public void RXM001_MethodInvocationUsedAsAnExpressionArgument()
    {
        const string invocation = "this.WhenChanging(GetMyExpression())";
        var source = SourceTemplate.Replace(WhenChangingPlaceholder, invocation);
        AssertDiagnostic(source, DiagnosticWarnings.ExpressionMustBeInline);
    }

    /// <summary>
    /// Expression arguments may not be specified as a method pointing to the actual expression.
    /// Yes: this.WhenChanging(x => x.Value).
    /// No: this.WhenChanging(GetMyExpression()).
    /// </summary>
    [TestMethod]
    public void RXM001_MethodInvocationUsedAsAnExpressionArgument_MultiExpressionMultiChain()
    {
        const string invocation = "this.WhenChanging(x => x.Child.Value, GetMyExpression(), (a, b) => $\"{a}-{b}\")";
        var source = SourceTemplate.Replace(WhenChangingPlaceholder, invocation);
        AssertDiagnostic(source, DiagnosticWarnings.ExpressionMustBeInline);
    }

    /// <summary>
    /// Expressions may not include method invocation anywhere in the chain.
    /// Yes: this.WhenChanging(x => x.Child.Value).
    /// No: this.WhenChanging(x => x.GetChild().Value).
    /// No: this.WhenChanging(x => x.Child.GetValue()).
    /// </summary>
    [TestMethod]
    public void RXM002_MethodInvocationUsedInExpressionChain()
    {
        const string invocation = "this.WhenChanging(x => x.GetValue())";
        var source = SourceTemplate.Replace(WhenChangingPlaceholder, invocation);
        AssertDiagnostic(source, DiagnosticWarnings.OnlyPropertyAndFieldAccessAllowed);
    }

    /// <summary>
    /// Expressions may not include method invocation anywhere in the chain.
    /// Yes: this.WhenChanging(x => x.Child.Value).
    /// No: this.WhenChanging(x => x.GetChild().Value).
    /// No: this.WhenChanging(x => x.Child.GetValue()).
    /// </summary>
    [TestMethod]
    public void RXM002_MethodInvocationUsedInExpressionChain_MultiExpressionMultiChain()
    {
        const string invocation = "this.WhenChanging(x => x.Value, x => x.GetChild().Value, (a, b) => $\"{a}-{b}\")";
        var source = SourceTemplate.Replace(WhenChangingPlaceholder, invocation);
        AssertDiagnostic(source, DiagnosticWarnings.OnlyPropertyAndFieldAccessAllowed);
    }

    /// <summary>
    /// Expressions may not include indexer access anywhere in the chain.
    /// Yes: this.WhenChanging(x => x.Child.Value).
    /// No: this.WhenChanging(x => x.GetChild[0].Value).
    /// No: this.WhenChanging(x => x.Child.GetValue["key"]).
    /// </summary>
    [TestMethod]
    public void RXM002_IndexerAccessUsedInExpressionChain()
    {
        const string invocation = "this.WhenChanging(x => x.Values[0])";
        var source = SourceTemplate.Replace(WhenChangingPlaceholder, invocation);
        AssertDiagnostic(source, DiagnosticWarnings.OnlyPropertyAndFieldAccessAllowed);
    }

    /// <summary>
    /// Expressions may not include indexer access anywhere in the chain.
    /// Yes: this.WhenChanging(x => x.Child.Value).
    /// No: this.WhenChanging(x => x.GetChild[0].Value).
    /// No: this.WhenChanging(x => x.Child.GetValue["key"]).
    /// </summary>
    [TestMethod]
    public void RXM002_IndexerAccessUsedInExpressionChain_MultiExpressionMultiChain()
    {
        const string invocation = "this.WhenChanging(x => x.Value, x => x.Children[0].Value, (a, b) => $\"{a}-{b}\")";
        var source = SourceTemplate.Replace(WhenChangingPlaceholder, invocation);
        AssertDiagnostic(source, DiagnosticWarnings.OnlyPropertyAndFieldAccessAllowed);
    }

    /// <summary>
    /// Expressions may not exclude the lambda parameter.
    /// Yes: this.WhenChanging(x => x.Child.Value).
    /// No: this.WhenChanging(x => Child.Value).
    /// </summary>
    [TestMethod]
    public void RXM003_LambdaParameterNotUsed()
    {
        const string invocation = "this.WhenChanging(x => Value)";
        var source = SourceTemplate.Replace(WhenChangingPlaceholder, invocation);
        AssertDiagnostic(source, DiagnosticWarnings.LambdaParameterMustBeUsed);
    }

    /// <summary>
    /// Expressions may not exclude the lambda parameter.
    /// Yes: this.WhenChanging(x => x.Child.Value).
    /// No: this.WhenChanging(x => Child.Value).
    /// </summary>
    [TestMethod]
    public void RXM003_LambdaParameterNotUsed_MultiExpressionMultiChain()
    {
        const string invocation = "this.WhenChanging(x => x.Value, x => Child.Value, (a, b) => $\"{a}-{b}\")";
        var source = SourceTemplate.Replace(WhenChangingPlaceholder, invocation);
        AssertDiagnostic(source, DiagnosticWarnings.LambdaParameterMustBeUsed);
    }

    /// <summary>
    /// Explicit extension invocation is not possible when it involves a private/protected property.
    /// </summary>
    /// <param name="propertyAccess">The access modifier of the Value property in the host.</param>
    [DataTestMethod]
    [DataRow(Accessibility.Private)]
    [DataRow(Accessibility.Protected)]
    [DataRow(Accessibility.ProtectedOrInternal)]
    [DataRow(Accessibility.ProtectedAndInternal)]
    public void RXM006_ExplicitInvocationInvolvingNonAccessibleProperty(Accessibility propertyAccess)
    {
        var hostPropertyTypeInfo = new EmptyClassBuilder()
            .WithClassAccess(Accessibility.Public);
        var source = new WhenChangedHostBuilder()
            .WithClassName("HostClass")
            .WithInvocation(InvocationKind.Explicit, x => x.Value)
            .WithClassAccess(Accessibility.Public)
            .WithPropertyType(hostPropertyTypeInfo)
            .WithPropertyAccess(propertyAccess)
            .BuildRoot();
        source += hostPropertyTypeInfo.BuildRoot();
        AssertDiagnostic(source, DiagnosticWarnings.UnableToGenerateExtension);
    }

    /// <summary>
    /// Explicit extension invocation is not possible when it involves a non-accessible host type.
    /// </summary>
    /// <param name="hostTypeAccess">The access modifier of the host type.</param>
    [DataTestMethod]
    [DataRow(Accessibility.Private)]
    [DataRow(Accessibility.Protected)]
    [DataRow(Accessibility.ProtectedOrInternal)]
    [DataRow(Accessibility.ProtectedAndInternal)]
    public void RXM006_ExplicitInvocationInvolvingNonAccessibleHostType(Accessibility hostTypeAccess)
    {
        var hostPropertyTypeInfo = new EmptyClassBuilder()
            .WithClassAccess(Accessibility.Public);
        var hostTypeInfo = new WhenChangedHostBuilder()
            .WithClassName("HostClass")
            .WithInvocation(InvocationKind.Explicit, x => x.Value)
            .WithClassAccess(hostTypeAccess)
            .WithPropertyType(hostPropertyTypeInfo)
            .WithPropertyAccess(Accessibility.Public);
        var source = new EmptyClassBuilder()
            .WithClassName("HostContainer")
            .WithClassAccess(Accessibility.Public)
            .AddNestedClass(hostTypeInfo)
            .BuildRoot();
        source += hostPropertyTypeInfo.BuildRoot();
        AssertDiagnostic(source, DiagnosticWarnings.UnableToGenerateExtension);
    }

    private void AssertDiagnostic(string source, DiagnosticDescriptor expectedDiagnostic)
    {
        Action action = () => _compilationUtil.CheckDiagnostics(("Diagnostics.cs", source), expectedDiagnostic);
        action.Should().Throw<InvalidOperationException>();
    }

    internal static class Method
    {
        public const string GetChild = "GetChild()";
        public const string GetValue = "GetValue()";
        public const string GetMyExpression = "GetMyExpression()";
    }

    internal static class Property
    {
        public const string Child = "Child";
        public const string Value = "Value";
        public const string Children = "Children";
        public const string Values = "Values";
        public const string MyExpression = "MyExpression";
    }
}
