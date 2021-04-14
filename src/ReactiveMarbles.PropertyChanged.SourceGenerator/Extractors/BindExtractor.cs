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
            foreach (var invocationExpression in syntaxReceiver.BindMethods)
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

                bool hasConverters = false;
                var expressions = new List<(LambdaExpressionSyntax Expression, ArgumentSyntax Argument)>();

                if (invocationExpression.ArgumentList.Arguments.Count < 3)
                {
                    context.ReportDiagnostic(DiagnosticWarnings.BindingIncorrectNumberParameters, invocationExpression.GetLocation());
                    continue;
                }

                var targetType = model.GetTypeInfo(invocationExpression.ArgumentList.Arguments[0].Expression).ConvertedType;

                foreach (var argument in invocationExpression.ArgumentList.Arguments.Skip(1))
                {
                    var argumentType = model.GetTypeInfo(argument.Expression).ConvertedType;

                    if (argumentType is null)
                    {
                        context.ReportDiagnostic(DiagnosticWarnings.InvalidExpression, argument.GetLocation());
                        continue;
                    }

                    if (argumentType.Name.Equals("Func"))
                    {
                        hasConverters = true;
                    }

                    if (argumentType.Name.Equals("Expression"))
                    {
                        expressions.Add((argument.Expression as LambdaExpressionSyntax, argument));
                    }
                }

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

                if (!GeneratorHelpers.GetExpression(context, viewModelExpression, compilation, model, out var viewModelExpressionArgument))
                {
                    // The argument is evaluates to an expression but it's not inline (could be a variable, method invocation, etc).
                    context.ReportDiagnostic(DiagnosticWarnings.InvalidExpression, viewModelExpression.GetLocation());
                    continue;
                }

                if (!GeneratorHelpers.GetExpression(context, viewExpression, compilation, model, out var viewExpressionArgument))
                {
                    // The argument is evaluates to an expression but it's not inline (could be a variable, method invocation, etc).
                    context.ReportDiagnostic(DiagnosticWarnings.InvalidExpression, viewExpression.GetLocation());
                    continue;
                }

                var minAccessibility = methodSymbol.TypeArguments.Min(x => x.DeclaredAccessibility);

                yield return new BindInvocationInfo(viewModelExpressionArgument.InputType, targetType, minAccessibility, !viewModelExpressionArgument.ContainsPrivateOrProtectedMember, hasConverters, viewModelExpressionArgument, viewExpressionArgument);
                yield return new WhenChangedExpressionInvocationInfo(viewModelExpressionArgument.InputType, !viewModelExpressionArgument.ContainsPrivateOrProtectedMember, viewModelExpressionArgument);
                yield return new WhenChangedExpressionInvocationInfo(viewExpressionArgument.InputType, !viewExpressionArgument.ContainsPrivateOrProtectedMember, viewExpressionArgument);
            }
        }
    }
}
