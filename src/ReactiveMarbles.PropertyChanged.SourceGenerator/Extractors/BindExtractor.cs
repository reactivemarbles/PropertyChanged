// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    internal class BindExtractor : IExtractor
    {
        private const string ExtensionClassFullName = "BindExtensions";

        public IEnumerable<InvocationInfo> GetInvocations(GeneratorExecutionContext context, Compilation compilation, SyntaxReceiver syntaxReceiver)
        {
            foreach (var invocationExpression in syntaxReceiver.WhenChangedMethods)
            {
                var model = compilation.GetSemanticModel(invocationExpression.SyntaxTree);
                var symbol = model.GetSymbolInfo(invocationExpression).Symbol;

                if (symbol is not IMethodSymbol methodSymbol)
                {
                    continue;
                }

                if (!methodSymbol.ContainingType.ToDisplayString().Equals(ExtensionClassFullName))
                {
                    continue;
                }

                var hasConverters = invocationExpression.ArgumentList.Arguments.Any(x => model.GetTypeInfo(x).ConvertedType.Name.Equals("Func"));
                var expressions = invocationExpression.ArgumentList.Arguments.Where(x => model.GetTypeInfo(x).ConvertedType.Name.Equals("Expression")).Select(x => (Expression: x.Expression as LambdaExpressionSyntax, Argument: x)).ToList();

                if (expressions.Count != 2)
                {
                    context.ReportDiagnostic(DiagnosticWarnings.BindingIncorrectNumberParameters, invocationExpression.GetLocation());
                    continue;
                }

                var viewModelExpression = expressions[0].Expression;

                if (viewModelExpression is null)
                {
                    // The argument is evaluates to an expression but it's not inline (could be a variable, method invocation, etc).
                    context.ReportDiagnostic(DiagnosticWarnings.ExpressionMustBeInline, expressions[0].Argument.GetLocation());
                    continue;
                }

                var viewExpression = expressions[1].Expression;

                if (viewExpression is null)
                {
                    // The argument is evaluates to an expression but it's not inline (could be a variable, method invocation, etc).
                    context.ReportDiagnostic(DiagnosticWarnings.ExpressionMustBeInline, expressions[1].Argument.GetLocation());
                    continue;
                }

                if (!GeneratorHelpers.GetExpression(context, methodSymbol, viewModelExpression, compilation, model, out var viewModelExpressionArgument))
                {
                    // The argument is evaluates to an expression but it's not inline (could be a variable, method invocation, etc).
                    context.ReportDiagnostic(DiagnosticWarnings.InvalidExpression, viewModelExpression.GetLocation());
                    continue;
                }

                if (!GeneratorHelpers.GetExpression(context, methodSymbol, viewExpression, compilation, model, out var viewExpressionArgument))
                {
                    // The argument is evaluates to an expression but it's not inline (could be a variable, method invocation, etc).
                    context.ReportDiagnostic(DiagnosticWarnings.InvalidExpression, viewExpression.GetLocation());
                    continue;
                }

                var minAccessibility = methodSymbol.TypeArguments.Min(x => x.DeclaredAccessibility);

                yield return new BindInvocationInfo(viewModelExpressionArgument.InputType, minAccessibility, !viewModelExpressionArgument.ContainsPrivateOrProtectedMember, hasConverters, viewModelExpressionArgument, viewExpressionArgument);
                yield return new WhenChangedExpressionInvocationInfo(viewModelExpressionArgument.InputType, !viewModelExpressionArgument.ContainsPrivateOrProtectedMember, viewModelExpressionArgument);
                yield return new WhenChangedExpressionInvocationInfo(viewExpressionArgument.InputType, !viewExpressionArgument.ContainsPrivateOrProtectedMember, viewExpressionArgument);
            }
        }
    }
}
