// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    internal class BindExtractor : IExtractor
    {
        private const string ExtensionClassFullName = "BindExtensions";
        private const string BindName = "Bind";
        private const string OneWayBindName = "OneWayBind";

        public IEnumerable<InvocationInfo> GetInvocations(GeneratorExecutionContext context, Compilation compilation, SyntaxReceiver syntaxReceiver)
        {
            foreach (var invocationInfo in syntaxReceiver.BindMethods.SelectMany(invocationExpression => GenerateInvocation(context, compilation, invocationExpression, true)))
            {
                yield return invocationInfo;
            }

            foreach (var invocationInfo in syntaxReceiver.OneWayBindMethods.SelectMany(invocationExpression => GenerateInvocation(context, compilation, invocationExpression, false)))
            {
                yield return invocationInfo;
            }
        }

        private static IEnumerable<InvocationInfo> GenerateInvocation(GeneratorExecutionContext context, Compilation compilation, InvocationExpressionSyntax invocationExpression, bool isTwoWayBind)
        {
            var model = compilation.GetSemanticModel(invocationExpression.SyntaxTree);
            var symbol = model.GetSymbolInfo(invocationExpression).Symbol;

            if (symbol is not IMethodSymbol methodSymbol)
            {
                yield break;
            }

            if (!methodSymbol.ContainingType.ToDisplayString().Equals(ExtensionClassFullName))
            {
                yield break;
            }

            if (!methodSymbol.Name.Equals(OneWayBindName) && !methodSymbol.Name.Equals(BindName))
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

            var targetType = model.GetTypeInfo(invocationExpression.ArgumentList.Arguments[0].Expression).ConvertedType;

            foreach (var argument in invocationExpression.ArgumentList.Arguments.Skip(1))
            {
                var argumentType = model.GetTypeInfo(argument.Expression).ConvertedType;

                if (argumentType is null)
                {
                    context.ReportDiagnostic(DiagnosticWarnings.InvalidExpression, argument.GetLocation());
                    continue;
                }

                switch (argumentType.Name)
                {
                    case "Func":
                        hasConverters = true;
                        break;
                    case "Expression":
                        expressions.Add((argument.Expression as LambdaExpressionSyntax, argument));
                        break;
                }
            }

            if (expressions.Count != 2)
            {
                context.ReportDiagnostic(DiagnosticWarnings.BindingIncorrectNumberParameters, invocationExpression.GetLocation());
                yield break;
            }

            var viewModelExpression = expressions[0].Expression;

            if (viewModelExpression is null)
            {
                // The argument is evaluates to an expression but it's not inline (could be a variable, method invocation, etc).
                context.ReportDiagnostic(DiagnosticWarnings.ExpressionMustBeInline, expressions[0].Argument.GetLocation());
                yield break;
            }

            var viewExpression = expressions[1].Expression;

            if (viewExpression is null)
            {
                // The argument is evaluates to an expression but it's not inline (could be a variable, method invocation, etc).
                context.ReportDiagnostic(DiagnosticWarnings.ExpressionMustBeInline, expressions[1].Argument.GetLocation());
                yield break;
            }

            if (!GeneratorHelpers.GetExpression(context, viewModelExpression, compilation, model, out var viewModelExpressionArgument))
            {
                // The argument is evaluates to an expression but it's not inline (could be a variable, method invocation, etc).
                context.ReportDiagnostic(DiagnosticWarnings.InvalidExpression, viewModelExpression.GetLocation());
                yield break;
            }

            if (!GeneratorHelpers.GetExpression(context, viewExpression, compilation, model, out var viewExpressionArgument))
            {
                // The argument is evaluates to an expression but it's not inline (could be a variable, method invocation, etc).
                context.ReportDiagnostic(DiagnosticWarnings.InvalidExpression, viewExpression.GetLocation());
                yield break;
            }

            var accessModifier = methodSymbol.TypeArguments.GetMinVisibility();

            if (!viewModelExpressionArgument.ContainsPrivateOrProtectedMember && !viewExpressionArgument.ContainsPrivateOrProtectedMember)
            {
                yield return isTwoWayBind ? new ExtensionBindInvocationInfo(viewModelExpressionArgument.InputType, accessModifier, hasConverters, viewModelExpressionArgument, viewExpressionArgument) : new ExtensionOneWayBindInvocationInfo(viewModelExpressionArgument.InputType, accessModifier, hasConverters, viewModelExpressionArgument, viewExpressionArgument);
            }
            else
            {
                var namespaceName = viewModelExpressionArgument.InputType.GetNamespace();
                var ancestorClasses = viewModelExpressionArgument.InputType.GetAncestors();
                var name = viewModelExpressionArgument.InputType.Name;

                yield return isTwoWayBind ? new PartialBindInvocationInfo(namespaceName, name, ancestorClasses, viewModelExpressionArgument.InputType, accessModifier, hasConverters, viewModelExpressionArgument, viewExpressionArgument) : new PartialOneWayBindInvocationInfo(namespaceName, name, ancestorClasses, viewModelExpressionArgument.InputType, accessModifier, hasConverters, viewModelExpressionArgument, viewExpressionArgument);
            }

            var isExplicitInvocation = methodSymbol.MethodKind == MethodKind.Ordinary;

            yield return new WhenChangedExpressionInvocationInfo(viewModelExpressionArgument.InputType, !viewModelExpressionArgument.ContainsPrivateOrProtectedMember, isExplicitInvocation, viewModelExpressionArgument);
            yield return new WhenChangedExpressionInvocationInfo(viewExpressionArgument.InputType, !viewExpressionArgument.ContainsPrivateOrProtectedMember, isExplicitInvocation, viewExpressionArgument);
        }
    }
}
