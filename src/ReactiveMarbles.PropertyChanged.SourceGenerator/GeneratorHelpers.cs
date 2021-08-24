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
    internal static class GeneratorHelpers
    {
        public static bool ContainsPrivateOrProtectedMember(Compilation compilation, SemanticModel model, LambdaExpressionSyntax lambdaExpression)
        {
            var members = new List<ExpressionSyntax>();
            var expression = lambdaExpression.ExpressionBody;
            var expressionChain = expression as MemberAccessExpressionSyntax;

            if (expression is null)
            {
                return false;
            }

            while (expressionChain != null)
            {
                members.Add(expression);
                expression = expressionChain.Expression;
                expressionChain = expression as MemberAccessExpressionSyntax;
            }

            members.Add(expression);
            members.Reverse();
            var inputTypeSymbol = model.GetTypeInfo(expression).ConvertedType;

            var current = inputTypeSymbol;

            while (current != null)
            {
                if (current.DeclaredAccessibility.IsPrivateOrProtected())
                {
                    return true;
                }

                current = current.ContainingType;
            }

            for (var i = members.Count - 1; i > 0; i--)
            {
                var parent = members[i - 1];
                var child = members[i];
                var parentTypeSymbol = model.GetTypeInfo(parent).ConvertedType;

                if (!SymbolEqualityComparer.Default.Equals(parentTypeSymbol, inputTypeSymbol))
                {
                    continue;
                }

                var propertyInvocationSymbol = model.GetSymbolInfo(child).Symbol;
                if (propertyInvocationSymbol is null)
                {
                    return false;
                }

                var propertyDeclarationSyntax = propertyInvocationSymbol.DeclaringSyntaxReferences[0].GetSyntax();
                var propertyDeclarationModel = compilation.GetSemanticModel(propertyDeclarationSyntax.SyntaxTree);
                var propertyDeclarationSymbol = propertyDeclarationModel.GetDeclaredSymbol(propertyDeclarationSyntax);

                if (propertyDeclarationSymbol is null)
                {
                    return false;
                }

                var propertyAccessibility = propertyDeclarationSymbol.DeclaredAccessibility;

                if (propertyAccessibility.IsPrivateOrProtected())
                {
                    return true;
                }

                var childTypeSymbol = model.GetTypeInfo(child).ConvertedType;

                if (childTypeSymbol is null)
                {
                    return false;
                }

                var childTypeAccess = childTypeSymbol.GetVisibility();
                if (childTypeAccess.IsPrivateOrProtected())
                {
                    return true;
                }
            }

            return false;
        }

        public static bool GetExpression(GeneratorExecutionContext context, LambdaExpressionSyntax lambdaExpression, Compilation compilation, SemanticModel model, out ExpressionArgument? expressionArgument)
        {
            var expressionChain = GetExpressionChain(context, lambdaExpression, model);

            expressionArgument = null;
            if (expressionChain == null)
            {
                return expressionChain != null;
            }

            var lambdaInputType = expressionChain[0].InputType;
            var lambdaOutputType = expressionChain[expressionChain.Count - 1].OutputType;
            var inputTypeAccess = lambdaInputType.GetVisibility();

            var containsPrivateOrProtectedMember =
                inputTypeAccess.IsPrivateOrProtected() ||
                ContainsPrivateOrProtectedMember(compilation, model, lambdaExpression);
            expressionArgument = new(lambdaExpression.Body.ToString(), expressionChain, lambdaInputType, lambdaOutputType, containsPrivateOrProtectedMember);

            return expressionChain != null;
        }

        public static MultiExpressionMethodDatum GetMultiExpression(IMethodSymbol methodSymbol)
        {
            var accessModifier = methodSymbol.TypeArguments.GetMinVisibility();

            var containsPrivateOrProtectedTypeArgument = accessModifier.IsPrivateOrProtected();
            var types = methodSymbol.TypeArguments;

            return new(accessModifier, types, containsPrivateOrProtectedTypeArgument);
        }

        public static List<ExpressionChain>? GetExpressionChain(GeneratorExecutionContext context, LambdaExpressionSyntax lambdaExpression, SemanticModel model)
        {
            var members = new List<ExpressionChain>();
            var expression = lambdaExpression.ExpressionBody;
            var expressionChain = expression as MemberAccessExpressionSyntax;

            while (expressionChain != null)
            {
                var name = expressionChain.Name.ToString();
                var inputType = model.GetTypeInfo(expressionChain.ChildNodes().ElementAt(0)).Type as INamedTypeSymbol;
                var outputType = model.GetTypeInfo(expressionChain.ChildNodes().ElementAt(1)).Type as INamedTypeSymbol;

                if (inputType is null)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticWarnings.LambdaParameterMustBeUsed,
                            lambdaExpression.Body.GetLocation()));

                    return null;
                }

                if (outputType is null)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticWarnings.LambdaParameterMustBeUsed,
                            lambdaExpression.Body.GetLocation()));

                    return null;
                }

                members.Add(new(name, inputType, outputType));

                expression = expressionChain.Expression;
                expressionChain = expression as MemberAccessExpressionSyntax;
            }

            if (expression is null)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticWarnings.LambdaParameterMustBeUsed,
                        lambdaExpression.Body.GetLocation()));

                return null;
            }

            if (expression is not IdentifierNameSyntax firstLinkInChain)
            {
                // It stopped before reaching the lambda parameter, so the expression is invalid.
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticWarnings.OnlyPropertyAndFieldAccessAllowed,
                        expression.GetLocation()));

                return null;
            }

            var lambdaParameterName =
                (lambdaExpression as SimpleLambdaExpressionSyntax)?.Parameter.Identifier.ToString() ??
                (lambdaExpression as ParenthesizedLambdaExpressionSyntax)?.ParameterList.Parameters[0].Identifier.ToString();

            if (string.Equals(lambdaParameterName, firstLinkInChain.Identifier.ToString(), StringComparison.InvariantCulture))
            {
                members.Reverse();
                return members;
            }

            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticWarnings.LambdaParameterMustBeUsed,
                    lambdaExpression.Body.GetLocation()));

            return null;
        }
    }
}
