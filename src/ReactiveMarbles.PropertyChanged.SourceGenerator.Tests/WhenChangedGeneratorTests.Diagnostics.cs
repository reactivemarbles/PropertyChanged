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
    /// Source generator tets.
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
        /// <param name="useRoslyn">A value indicating whether the Roslyn implementation should be used.</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void RXM001_PropertyInvocationUsedAsAnExpressionArgument(bool useRoslyn)
        {
            var invocation = "this.WhenChanged(MyExpression)";
            var source = SourceTemplate.Replace(WhenChangedPlaceholder, invocation);
            AssertDiagnostic(source, DiagnosticWarnings.ExpressionMustBeInline, useRoslyn);
        }

        /// <summary>
        /// Expression arguments may not be specified as a property pointing to the actual expression.
        /// Yes: this.WhenChanged(x => x.Value).
        /// No: this.WhenChanged(MyExpression).
        /// </summary>
        /// <param name="useRoslyn">A value indicating whether the Roslyn implementation should be used.</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void RXM001_PropertyInvocationUsedAsAnExpressionArgument_MultiExpressionMultiChain(bool useRoslyn)
        {
            var invocation = "this.WhenChanged(x => x.Child.Value, MyExpression, (a, b) => $\"{a}-{b}\")";
            var source = SourceTemplate.Replace(WhenChangedPlaceholder, invocation);
            AssertDiagnostic(source, DiagnosticWarnings.ExpressionMustBeInline, useRoslyn);
        }

        /// <summary>
        /// Expression arguments may not be specified as a method pointing to the actual expression.
        /// Yes: this.WhenChanged(x => x.Value).
        /// No: this.WhenChanged(GetMyExpression()).
        /// </summary>
        /// <param name="useRoslyn">A value indicating whether the Roslyn implementation should be used.</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void RXM001_MethodInvocationUsedAsAnExpressionArgument(bool useRoslyn)
        {
            var invocation = "this.WhenChanged(GetMyExpression())";
            var source = SourceTemplate.Replace(WhenChangedPlaceholder, invocation);
            AssertDiagnostic(source, DiagnosticWarnings.ExpressionMustBeInline, useRoslyn);
        }

        /// <summary>
        /// Expression arguments may not be specified as a method pointing to the actual expression.
        /// Yes: this.WhenChanged(x => x.Value).
        /// No: this.WhenChanged(GetMyExpression()).
        /// </summary>
        /// <param name="useRoslyn">A value indicating whether the Roslyn implementation should be used.</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void RXM001_MethodInvocationUsedAsAnExpressionArgument_MultiExpressionMultiChain(bool useRoslyn)
        {
            var invocation = "this.WhenChanged(x => x.Child.Value, GetMyExpression(), (a, b) => $\"{a}-{b}\")";
            var source = SourceTemplate.Replace(WhenChangedPlaceholder, invocation);
            AssertDiagnostic(source, DiagnosticWarnings.ExpressionMustBeInline, useRoslyn);
        }

        /// <summary>
        /// Expressions may not include method invocation anywhere in the chain.
        /// Yes: this.WhenChanged(x => x.Child.Value).
        /// No: this.WhenChanged(x => x.GetChild().Value).
        /// No: this.WhenChanged(x => x.Child.GetValue()).
        /// </summary>
        /// <param name="useRoslyn">A value indicating whether the Roslyn implementation should be used.</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void RXM002_MethodInvocationUsedInExpressionChain(bool useRoslyn)
        {
            var invocation = "this.WhenChanged(x => x.GetValue())";
            var source = SourceTemplate.Replace(WhenChangedPlaceholder, invocation);
            AssertDiagnostic(source, DiagnosticWarnings.OnlyPropertyAndFieldAccessAllowed, useRoslyn);
        }

        /// <summary>
        /// Expressions may not include method invocation anywhere in the chain.
        /// Yes: this.WhenChanged(x => x.Child.Value).
        /// No: this.WhenChanged(x => x.GetChild().Value).
        /// No: this.WhenChanged(x => x.Child.GetValue()).
        /// </summary>
        /// <param name="useRoslyn">A value indicating whether the Roslyn implementation should be used.</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void RXM002_MethodInvocationUsedInExpressionChain_MultiExpressionMultiChain(bool useRoslyn)
        {
            var invocation = "this.WhenChanged(x => x.Value, x => x.GetChild().Value, (a, b) => $\"{a}-{b}\")";
            var source = SourceTemplate.Replace(WhenChangedPlaceholder, invocation);
            AssertDiagnostic(source, DiagnosticWarnings.OnlyPropertyAndFieldAccessAllowed, useRoslyn);
        }

        /// <summary>
        /// Expressions may not include indexer access anywhere in the chain.
        /// Yes: this.WhenChanged(x => x.Child.Value).
        /// No: this.WhenChanged(x => x.GetChild[0].Value).
        /// No: this.WhenChanged(x => x.Child.GetValue["key"]).
        /// </summary>
        /// <param name="useRoslyn">A value indicating whether the Roslyn implementation should be used.</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void RXM002_IndexerAccessUsedInExpressionChain(bool useRoslyn)
        {
            var invocation = "this.WhenChanged(x => x.Values[0])";
            var source = SourceTemplate.Replace(WhenChangedPlaceholder, invocation);
            AssertDiagnostic(source, DiagnosticWarnings.OnlyPropertyAndFieldAccessAllowed, useRoslyn);
        }

        /// <summary>
        /// Expressions may not include indexer access anywhere in the chain.
        /// Yes: this.WhenChanged(x => x.Child.Value).
        /// No: this.WhenChanged(x => x.GetChild[0].Value).
        /// No: this.WhenChanged(x => x.Child.GetValue["key"]).
        /// </summary>
        /// <param name="useRoslyn">A value indicating whether the Roslyn implementation should be used.</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void RXM002_IndexerAccessUsedInExpressionChain_MultiExpressionMultiChain(bool useRoslyn)
        {
            var invocation = "this.WhenChanged(x => x.Value, x => x.Children[0].Value, (a, b) => $\"{a}-{b}\")";
            var source = SourceTemplate.Replace(WhenChangedPlaceholder, invocation);
            AssertDiagnostic(source, DiagnosticWarnings.OnlyPropertyAndFieldAccessAllowed, useRoslyn);
        }

        /// <summary>
        /// Expressions may not exclude the lambda parameter.
        /// Yes: this.WhenChanged(x => x.Child.Value).
        /// No: this.WhenChanged(x => Child.Value).
        /// </summary>
        /// <param name="useRoslyn">A value indicating whether the Roslyn implementation should be used.</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void RXM003_LambdaParameterNotUsed(bool useRoslyn)
        {
            var invocation = "this.WhenChanged(x => Value)";
            var source = SourceTemplate.Replace(WhenChangedPlaceholder, invocation);
            AssertDiagnostic(source, DiagnosticWarnings.LambdaParameterMustBeUsed, useRoslyn);
        }

        /// <summary>
        /// Expressions may not exclude the lambda parameter.
        /// Yes: this.WhenChanged(x => x.Child.Value).
        /// No: this.WhenChanged(x => Child.Value).
        /// </summary>
        /// <param name="useRoslyn">A value indicating whether the Roslyn implementation should be used.</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void RXM003_LambdaParameterNotUsed_MultiExpressionMultiChain(bool useRoslyn)
        {
            var invocation = "this.WhenChanged(x => x.Value, x => Child.Value, (a, b) => $\"{a}-{b}\")";
            var source = SourceTemplate.Replace(WhenChangedPlaceholder, invocation);
            AssertDiagnostic(source, DiagnosticWarnings.LambdaParameterMustBeUsed, useRoslyn);
        }

        private static void AssertDiagnostic(string source, DiagnosticDescriptor expectedDiagnostic, bool useRoslyn)
        {
            Compilation compilation = CompilationUtil.CreateCompilation(source);
            var newCompilation = CompilationUtil.RunGenerators(compilation, out var generatorDiagnostics, new Generator(useRoslyn));
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
