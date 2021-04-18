// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    internal class WhenChangedExtractor : IExtractor
    {
        private const string ExtensionClassFullName = "NotifyPropertyChangedExtensions";

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

                foreach (var argument in invocationExpression.ArgumentList.Arguments.Where(argument => model.GetTypeInfo(argument.Expression).ConvertedType.Name.Equals("Expression")))
                {
                    if (argument.Expression is LambdaExpressionSyntax lambdaExpression)
                    {
                        var isValid = GeneratorHelpers.GetExpression(context, lambdaExpression, compilation, model, out var expressionArgument);

                        if (!isValid)
                        {
                            continue;
                        }

                        yield return new WhenChangedExpressionInvocationInfo(expressionArgument.InputType, !expressionArgument.ContainsPrivateOrProtectedMember, expressionArgument);
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
