// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    internal static class ExtractorHelpers
    {
        internal static IEnumerable<TypeDatum> GenerateBindInvocation(string extensionClass, GeneratorExecutionContext context, Compilation compilation, InvocationExpressionSyntax invocationExpression, bool isTwoWayBind)
        {
            var model = compilation.GetSemanticModel(invocationExpression.SyntaxTree);
            var symbol = model.GetSymbolInfo(invocationExpression).Symbol;

            if (symbol is not IMethodSymbol methodSymbol)
            {
                yield break;
            }

            if (!methodSymbol.ContainingType.ToDisplayString().Equals(extensionClass))
            {
                yield break;
            }

            var hasConverters = false;
            var expressions = new List<(LambdaExpressionSyntax Expression, ArgumentSyntax Argument)>();

            if (invocationExpression.ArgumentList.Arguments.Count < 3)
            {
                context.ReportDiagnostic(DiagnosticWarnings.BindingIncorrectNumberParameters, invocationExpression.GetLocation());
                yield break;
            }

            foreach (var argument in invocationExpression.ArgumentList.Arguments.Skip(1))
            {
                var argumentType = model.GetTypeInfo(argument.Expression).ConvertedType;

                if (argumentType is null)
                {
                    context.ReportDiagnostic(DiagnosticWarnings.InvalidExpression, argument.GetLocation());
                    continue;
                }

                if (argumentType.Name == "Func")
                {
                    hasConverters = true;
                    break;
                }

                if (argumentType.Name == "Expression" && argument.Expression is LambdaExpressionSyntax lambdaExpression)
                {
                    expressions.Add((lambdaExpression, argument));
                }
            }

            if (expressions.Count != 2)
            {
                context.ReportDiagnostic(DiagnosticWarnings.BindingIncorrectNumberParameters, invocationExpression.GetLocation());
                yield break;
            }

            var hostExpression = expressions[0].Expression;

            if (hostExpression is null)
            {
                // The argument is evaluates to an expression but it's not inline (could be a variable, method invocation, etc).
                context.ReportDiagnostic(DiagnosticWarnings.ExpressionMustBeInline, expressions[0].Argument.GetLocation());
                yield break;
            }

            var targetExpression = expressions[1].Expression;

            if (targetExpression is null)
            {
                // The argument is evaluates to an expression but it's not inline (could be a variable, method invocation, etc).
                context.ReportDiagnostic(DiagnosticWarnings.ExpressionMustBeInline, expressions[1].Argument.GetLocation());
                yield break;
            }

            if (!GeneratorHelpers.GetExpression(context, hostExpression, compilation, model, out var hostExpressionArgument))
            {
                // The argument is evaluates to an expression but it's not inline (could be a variable, method invocation, etc).
                context.ReportDiagnostic(DiagnosticWarnings.InvalidExpression, hostExpression.GetLocation());
                yield break;
            }

            if (!GeneratorHelpers.GetExpression(context, targetExpression, compilation, model, out var targetExpressionArgument))
            {
                // The argument is evaluates to an expression but it's not inline (could be a variable, method invocation, etc).
                context.ReportDiagnostic(DiagnosticWarnings.InvalidExpression, targetExpression.GetLocation());
                yield break;
            }

            var accessModifier = methodSymbol.TypeArguments.GetMinVisibility();

            if (hostExpressionArgument is null || targetExpressionArgument is null)
            {
                // The argument is evaluates to an expression but it's not inline (could be a variable, method invocation, etc).
                context.ReportDiagnostic(DiagnosticWarnings.InvalidExpression, hostExpression.GetLocation());
                yield break;
            }

            if (!hostExpressionArgument.ContainsPrivateOrProtectedMember && !targetExpressionArgument.ContainsPrivateOrProtectedMember)
            {
                yield return isTwoWayBind ? new ExtensionBindInvocationInfo(hostExpressionArgument.InputType, accessModifier, hasConverters, hostExpressionArgument, targetExpressionArgument) : new ExtensionOneWayBindInvocationInfo(hostExpressionArgument.InputType, accessModifier, hasConverters, hostExpressionArgument, targetExpressionArgument);
            }
            else
            {
                var namespaceName = hostExpressionArgument.InputType.GetNamespace();
                var ancestorClasses = hostExpressionArgument.InputType.GetAncestors();
                var name = hostExpressionArgument.InputType.Name;

                yield return isTwoWayBind ? new PartialBindInvocationInfo(namespaceName, name, ancestorClasses, hostExpressionArgument.InputType, accessModifier, hasConverters, hostExpressionArgument, targetExpressionArgument) : new PartialOneWayBindInvocationInfo(namespaceName, name, ancestorClasses, hostExpressionArgument.InputType, accessModifier, hasConverters, hostExpressionArgument, targetExpressionArgument);
            }

            var isExplicitInvocation = methodSymbol.MethodKind == MethodKind.Ordinary;

            yield return new WhenChangedExpressionInvocationInfo(hostExpressionArgument.InputType, !hostExpressionArgument.ContainsPrivateOrProtectedMember, isExplicitInvocation, hostExpressionArgument);
            yield return new WhenChangedExpressionInvocationInfo(targetExpressionArgument.InputType, !targetExpressionArgument.ContainsPrivateOrProtectedMember, isExplicitInvocation, targetExpressionArgument);
        }
    }
}
