// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

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
        private const string BindPlaceholder = "[bind_invocation]";
        private const string SourceTemplate = @"
using System;
using System.ComponentModel;
using System.Linq.Expressions;

public class View : INotifyPropertyChanged
{
    public View()
    {
        [bind_invocation];
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public ViewModel ViewModel { get; }
    public View Child { get; }
    public string Value { get; }
    public View[] Children { get; }
    public string[] Values { get; }
    public Expression<Func<View, string>> ViewExpression => x => x.Value;
    public Expression<Func<ViewModel, string>> ViewModelExpression => x => x.Value;
    public Expression<Func<View, string>> GetViewExpression() => x => x.Value;
    public Expression<Func<ViewModel, string>> GetViewModelExpression() => x => x.Value;
    public View GetChild() => Child;
    public string GetValue() => Value;
}

public class ViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    public ViewModel Child { get; }
    public string Value { get; }
    public ViewModel[] Children { get; }
    public string[] Values { get; }
    public ViewModel GetChild() => Child;
    public string GetValue() => Value;
}
";

        /// <summary>
        /// Expression arguments may not be specified as a property pointing to the actual expression.
        /// Yes: this.Bind(ViewModel, x => x.Value, x => x.Value).
        /// No: this.Bind(ViewModel, ViewExpression, x => x.Value).
        /// </summary>
        [Fact]
        public void RXM001_PropertyInvocationUsedAsAnExpressionArgument()
        {
            var invocation = "this.Bind(ViewModel, ViewExpression, ViewModelExpression)";
            var source = SourceTemplate.Replace(BindPlaceholder, invocation);
            AssertDiagnostic(source, DiagnosticWarnings.ExpressionMustBeInline);
        }

        /// <summary>
        /// Expression arguments may not be specified as a method pointing to the actual expression.
        /// Yes: this.Bind(ViewModel, x => x.Value, x => x.Value).
        /// No: this.Bind(ViewModel, GetViewExpression(), x => x.Value).
        /// </summary>
        [Fact]
        public void RXM001_MethodInvocationUsedAsAnExpressionArgument()
        {
            var invocation = "this.Bind(ViewModel, GetViewExpression(), GetViewModelExpression())";
            var source = SourceTemplate.Replace(BindPlaceholder, invocation);
            AssertDiagnostic(source, DiagnosticWarnings.ExpressionMustBeInline);
        }

        private void AssertDiagnostic(string source, DiagnosticDescriptor expectedDiagnostic) => _compilationUtil.CheckDiagnostics(source, expectedDiagnostic);
    }
}
