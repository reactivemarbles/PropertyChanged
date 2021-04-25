// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq;
using System.Reactive.Linq;
using Microsoft.CodeAnalysis;
using ReactiveMarbles.PropertyChanged.SourceGenerator.Builders;
using Xunit;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Tests
{
    /// <summary>
    /// WhenChanged tests.
    /// </summary>
    public partial class WhenChangedGeneratorTests
    {
        private const string WhenChangedPlaceholder = "[whenchanged_invocation]";
        private const string SourceTemplate = @"
using System;
using System.ComponentModel;
using System.Linq.Expressions;

public class HostClass : INotifyPropertyChanged
{
    public HostClass()
    {
        [whenchanged_invocation];
    }

    public event PropertyChangedEventHandler PropertyChanged;
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

        /// <summary>
        /// Expression arguments may not be specified as a property pointing to the actual expression.
        /// Yes: this.WhenChanged(x => x.Value).
        /// No: this.WhenChanged(MyExpression).
        /// </summary>
        [Fact]
        public void RXM001_PropertyInvocationUsedAsAnExpressionArgument()
        {
            var invocation = "this.WhenChanged(MyExpression)";
            var source = SourceTemplate.Replace(WhenChangedPlaceholder, invocation);
            AssertDiagnostic(source, DiagnosticWarnings.ExpressionMustBeInline);
        }

        /// <summary>
        /// Expression arguments may not be specified as a property pointing to the actual expression.
        /// Yes: this.WhenChanged(x => x.Value).
        /// No: this.WhenChanged(MyExpression).
        /// </summary>
        [Fact]
        public void RXM001_PropertyInvocationUsedAsAnExpressionArgument_MultiExpressionMultiChain()
        {
            var invocation = "this.WhenChanged(x => x.Child.Value, MyExpression, (a, b) => $\"{a}-{b}\")";
            var source = SourceTemplate.Replace(WhenChangedPlaceholder, invocation);
            AssertDiagnostic(source, DiagnosticWarnings.ExpressionMustBeInline);
        }

        /// <summary>
        /// Expression arguments may not be specified as a method pointing to the actual expression.
        /// Yes: this.WhenChanged(x => x.Value).
        /// No: this.WhenChanged(GetMyExpression()).
        /// </summary>
        [Fact]
        public void RXM001_MethodInvocationUsedAsAnExpressionArgument()
        {
            var invocation = "this.WhenChanged(GetMyExpression())";
            var source = SourceTemplate.Replace(WhenChangedPlaceholder, invocation);
            AssertDiagnostic(source, DiagnosticWarnings.ExpressionMustBeInline);
        }

        /// <summary>
        /// Expression arguments may not be specified as a method pointing to the actual expression.
        /// Yes: this.WhenChanged(x => x.Value).
        /// No: this.WhenChanged(GetMyExpression()).
        /// </summary>
        [Fact]
        public void RXM001_MethodInvocationUsedAsAnExpressionArgument_MultiExpressionMultiChain()
        {
            var invocation = "this.WhenChanged(x => x.Child.Value, GetMyExpression(), (a, b) => $\"{a}-{b}\")";
            var source = SourceTemplate.Replace(WhenChangedPlaceholder, invocation);
            AssertDiagnostic(source, DiagnosticWarnings.ExpressionMustBeInline);
        }

        /// <summary>
        /// Expressions may not include method invocation anywhere in the chain.
        /// Yes: this.WhenChanged(x => x.Child.Value).
        /// No: this.WhenChanged(x => x.GetChild().Value).
        /// No: this.WhenChanged(x => x.Child.GetValue()).
        /// </summary>
        [Fact]
        public void RXM002_MethodInvocationUsedInExpressionChain()
        {
            var invocation = "this.WhenChanged(x => x.GetValue())";
            var source = SourceTemplate.Replace(WhenChangedPlaceholder, invocation);
            AssertDiagnostic(source, DiagnosticWarnings.OnlyPropertyAndFieldAccessAllowed);
        }

        /// <summary>
        /// Expressions may not include method invocation anywhere in the chain.
        /// Yes: this.WhenChanged(x => x.Child.Value).
        /// No: this.WhenChanged(x => x.GetChild().Value).
        /// No: this.WhenChanged(x => x.Child.GetValue()).
        /// </summary>
        [Fact]
        public void RXM002_MethodInvocationUsedInExpressionChain_MultiExpressionMultiChain()
        {
            var invocation = "this.WhenChanged(x => x.Value, x => x.GetChild().Value, (a, b) => $\"{a}-{b}\")";
            var source = SourceTemplate.Replace(WhenChangedPlaceholder, invocation);
            AssertDiagnostic(source, DiagnosticWarnings.OnlyPropertyAndFieldAccessAllowed);
        }

        /// <summary>
        /// Expressions may not include indexer access anywhere in the chain.
        /// Yes: this.WhenChanged(x => x.Child.Value).
        /// No: this.WhenChanged(x => x.GetChild[0].Value).
        /// No: this.WhenChanged(x => x.Child.GetValue["key"]).
        /// </summary>
        [Fact]
        public void RXM002_IndexerAccessUsedInExpressionChain()
        {
            var invocation = "this.WhenChanged(x => x.Values[0])";
            var source = SourceTemplate.Replace(WhenChangedPlaceholder, invocation);
            AssertDiagnostic(source, DiagnosticWarnings.OnlyPropertyAndFieldAccessAllowed);
        }

        /// <summary>
        /// Expressions may not include indexer access anywhere in the chain.
        /// Yes: this.WhenChanged(x => x.Child.Value).
        /// No: this.WhenChanged(x => x.GetChild[0].Value).
        /// No: this.WhenChanged(x => x.Child.GetValue["key"]).
        /// </summary>
        [Fact]
        public void RXM002_IndexerAccessUsedInExpressionChain_MultiExpressionMultiChain()
        {
            var invocation = "this.WhenChanged(x => x.Value, x => x.Children[0].Value, (a, b) => $\"{a}-{b}\")";
            var source = SourceTemplate.Replace(WhenChangedPlaceholder, invocation);
            AssertDiagnostic(source, DiagnosticWarnings.OnlyPropertyAndFieldAccessAllowed);
        }

        /// <summary>
        /// Expressions may not exclude the lambda parameter.
        /// Yes: this.WhenChanged(x => x.Child.Value).
        /// No: this.WhenChanged(x => Child.Value).
        /// </summary>
        [Fact]
        public void RXM003_LambdaParameterNotUsed()
        {
            var invocation = "this.WhenChanged(x => Value)";
            var source = SourceTemplate.Replace(WhenChangedPlaceholder, invocation);
            AssertDiagnostic(source, DiagnosticWarnings.LambdaParameterMustBeUsed);
        }

        /// <summary>
        /// Expressions may not exclude the lambda parameter.
        /// Yes: this.WhenChanged(x => x.Child.Value).
        /// No: this.WhenChanged(x => Child.Value).
        /// </summary>
        [Fact]
        public void RXM003_LambdaParameterNotUsed_MultiExpressionMultiChain()
        {
            var invocation = "this.WhenChanged(x => x.Value, x => Child.Value, (a, b) => $\"{a}-{b}\")";
            var source = SourceTemplate.Replace(WhenChangedPlaceholder, invocation);
            AssertDiagnostic(source, DiagnosticWarnings.LambdaParameterMustBeUsed);
        }

        /// <summary>
        /// Explicit extension invocation is not possible when it involves a private/protected property.
        /// </summary>
        /// <param name="propertyAccess">The access modifier of the Value property in the host.</param>
        [Theory]
        [InlineData(Accessibility.Private)]
        [InlineData(Accessibility.Protected)]
        [InlineData(Accessibility.ProtectedOrInternal)]
        [InlineData(Accessibility.ProtectedAndInternal)]
        public void RXM006_ExplicitInvocationInvolvingNonAccessibleProperty(Accessibility propertyAccess)
        {
            var hostPropertyTypeInfo = new EmptyClassBuilder()
                .WithClassAccess(Accessibility.Public);
            var source = new WhenChangedHostBuilder()
                .WithClassName("Host")
                .WithInvocation(InvocationKind.Explicit, ReceiverKind.This, x => x.Value)
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
        [Theory]
        [InlineData(Accessibility.Private)]
        [InlineData(Accessibility.Protected)]
        [InlineData(Accessibility.ProtectedOrInternal)]
        [InlineData(Accessibility.ProtectedAndInternal)]
        public void RXM006_ExplicitInvocationInvolvingNonAccessibleHostType(Accessibility hostTypeAccess)
        {
            var hostPropertyTypeInfo = new EmptyClassBuilder()
                .WithClassAccess(Accessibility.Public);
            var hostTypeInfo = new WhenChangedHostBuilder()
                .WithClassName("Host")
                .WithInvocation(InvocationKind.Explicit, ReceiverKind.This, x => x.Value)
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

        private static void AssertDiagnostic(string source, DiagnosticDescriptor expectedDiagnostic)
        {
            Compilation compilation = CompilationUtil.CreateCompilation(source);
            var newCompilation = CompilationUtil.RunGenerators(compilation, out var generatorDiagnostics, new Generator());
            var compilationDiagnostics = newCompilation.GetDiagnostics();
            var compilationErrors = compilationDiagnostics.Where(x => x.Severity == DiagnosticSeverity.Error).Select(x => x.GetMessage()).ToList();

            Assert.Empty(compilationErrors);
            Assert.Single(generatorDiagnostics);
            Assert.Equal(expectedDiagnostic, generatorDiagnostics[0].Descriptor);
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
}
