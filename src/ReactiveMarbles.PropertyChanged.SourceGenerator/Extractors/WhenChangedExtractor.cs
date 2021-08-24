// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    internal class WhenChangedExtractor : IExtractor
    {
        private readonly string _extensionClassFullName;
        private readonly Func<SyntaxReceiver, List<InvocationExpressionSyntax>> _invocationListSelector;

        public WhenChangedExtractor(string extensionClassFullName, Func<SyntaxReceiver, List<InvocationExpressionSyntax>> invocationListSelector)
        {
            _extensionClassFullName = extensionClassFullName;
            _invocationListSelector = invocationListSelector;
        }

        public IEnumerable<TypeDatum> GetInvocations(GeneratorExecutionContext context, Compilation compilation, SyntaxReceiver syntaxReceiver)
        {
            foreach (var invocationExpression in _invocationListSelector.Invoke(syntaxReceiver))
            {
                var model = compilation.GetSemanticModel(invocationExpression.SyntaxTree);
                var symbol = model.GetSymbolInfo(invocationExpression).Symbol;

                if (symbol is not IMethodSymbol methodSymbol)
                {
                    continue;
                }

                if (!methodSymbol.ContainingType.ToDisplayString().Equals(_extensionClassFullName))
                {
                    continue;
                }

                var invocationArguments = invocationExpression.ArgumentList.Arguments.Where(argument => model.GetTypeInfo(argument.Expression).ConvertedType?.Name.Equals("Expression") == true);

                foreach (var argument in invocationArguments)
                {
                    if (argument.Expression is LambdaExpressionSyntax lambdaExpression)
                    {
                        var isValid = GeneratorHelpers.GetExpression(context, lambdaExpression, compilation, model, out var expressionArgument);

                        if (!isValid)
                        {
                            continue;
                        }

                        if (expressionArgument is null)
                        {
                            continue;
                        }

                        // this.WhenChanged(...) and instance.WhenChanged(...) MethodKind = MethodKind.ReducedExtension
                        // NotifyPropertyChangedExtensions.WhenChanged(...) MethodKind = MethodKind.Ordinary
                        // An alternative way is checking if methodSymbol.ReceiverType.Name == NotifyPropertyChangedExtensions.
                        var isExplicitInvocation = methodSymbol.MethodKind == MethodKind.Ordinary;

                        if (isExplicitInvocation && expressionArgument.ContainsPrivateOrProtectedMember)
                        {
                            context.ReportDiagnostic(DiagnosticWarnings.UnableToGenerateExtension, invocationExpression.GetLocation());
                            break;
                        }

                        yield return new WhenChangedExpressionInvocationInfo(expressionArgument.InputType, !expressionArgument.ContainsPrivateOrProtectedMember, isExplicitInvocation, expressionArgument);
                    }
                    else
                    {
                        // The argument is evaluates to an expression but it's not inline (could be a variable, method invocation, etc).
                        context.ReportDiagnostic(DiagnosticWarnings.ExpressionMustBeInline, argument.GetLocation());
                    }
                }

                if (methodSymbol.TypeArguments.Length > 2)
                {
                    var multiExpression = GeneratorHelpers.GetMultiExpression(methodSymbol);

                    yield return new WhenChangedMultiMethodInvocationInfo(multiExpression.InputType, !multiExpression.ContainsPrivateOrProtectedTypeArgument, multiExpression);
                }
            }
        }
    }
}
