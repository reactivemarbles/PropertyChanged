// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    internal static class DiagnosticWarnings
    {
        internal static readonly DiagnosticDescriptor ExpressionMustBeInline = new(
            "RXM001",
            "Expression chain must be inline",
            "The expression must be inline (e.g. not a variable or method invocation).",
            "Compiler",
            DiagnosticSeverity.Error,
            true);

        internal static readonly DiagnosticDescriptor OnlyPropertyAndFieldAccessAllowed = new(
            "RXM002",
            "Expression chain may only consist of property and field access",
            "The expression may only consist of field and property access",
            "Compiler",
            DiagnosticSeverity.Error,
            true);

        internal static readonly DiagnosticDescriptor LambdaParameterMustBeUsed = new(
            "RXM003",
            "Lambda parameter must be used in expression chain",
            "The lambda parameter must be used in the expression chain",
            "Compiler",
            DiagnosticSeverity.Error,
            true);

        internal static readonly DiagnosticDescriptor BindingIncorrectNumberParameters = new(
            "RXM004",
            "Must be both ViewModel and View expressions for Bind method",
            "Must be both ViewModel and View expressions for Bind method",
            "Compiler",
            DiagnosticSeverity.Error,
            true);

        internal static readonly DiagnosticDescriptor InvalidExpression = new(
            "RXM005",
            "The expression is not valid and does not point towards a valid item",
            "The expression is not valid and does not point towards a valid item",
            "Compiler",
            DiagnosticSeverity.Error,
            true);

        internal static readonly DiagnosticDescriptor UnableToGenerateExtension = new(
            "RXM006",
            "Unable to generate extension method",
            "Unable to generate extension method because the invocation involves one or more private/protected types or properties. Invoke via instance method, instead.",
            "Compiler",
            DiagnosticSeverity.Error,
            true);

        internal static readonly DiagnosticDescriptor InvalidNumberExpressions = new(
            "RXM007",
            "There are not enough expressions for Bind",
            "There are not enough expressions for Bind. There are {0} elements.",
            "Compiler",
            DiagnosticSeverity.Error,
            true);
    }
}
