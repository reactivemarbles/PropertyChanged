﻿// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
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
    internal static class GeneratorHelpers
    {
        public static bool ContainsPrivateOrProtectedMember(Compilation compilation, SemanticModel model, LambdaExpressionSyntax lambdaExpression)
        {
            var members = new List<ExpressionSyntax>();
            var expression = lambdaExpression.ExpressionBody;
            var expressionChain = expression as MemberAccessExpressionSyntax;

            while (expressionChain != null)
            {
                members.Add(expression);
                expression = expressionChain.Expression;
                expressionChain = expression as MemberAccessExpressionSyntax;
            }

            members.Add(expression);
            members.Reverse();
            var inputTypeSymbol = model.GetTypeInfo(expression).ConvertedType;

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
                var propertyDeclarationSyntax = propertyInvocationSymbol.DeclaringSyntaxReferences.First().GetSyntax();
                var propertyDeclarationModel = compilation.GetSemanticModel(propertyDeclarationSyntax.SyntaxTree);
                var propertyDeclarationSymbol = propertyDeclarationModel.GetDeclaredSymbol(propertyDeclarationSyntax);
                var propertyAccessibility = propertyDeclarationSymbol.DeclaredAccessibility;

                if (propertyAccessibility.IsPrivateOrProtected())
                {
                    return true;
                }

                var childTypeSymbol = model.GetTypeInfo(child).ConvertedType;
                var childTypeAccess = childTypeSymbol.GetVisibility();
                if (childTypeAccess.IsPrivateOrProtected())
                {
                    return true;
                }
            }

            return false;
        }

        public static MethodDatum CreateSingleExpressionMethodDatum(List<ExpressionArgument> outputTypeGroup)
        {
            MethodDatum methodDatum = null;
            var (_, expressionChain, inputTypeSymbol, outputTypeSymbol, _) = outputTypeGroup[0];
            var (inputTypeName, outputTypeName) = (inputTypeSymbol.ToDisplayString(), outputTypeSymbol.ToDisplayString());

            var accessModifier = inputTypeSymbol.DeclaredAccessibility;
            if (outputTypeSymbol.DeclaredAccessibility < inputTypeSymbol.DeclaredAccessibility)
            {
                accessModifier = outputTypeSymbol.DeclaredAccessibility;
            }

            switch (outputTypeGroup.Count)
            {
                case 1:
                    methodDatum = new SingleExpressionOptimizedImplMethodDatum(inputTypeName, outputTypeName, accessModifier, expressionChain);
                    break;
                case > 1:
                {
                    var mapName = $"__generated{inputTypeSymbol.GetVariableName()}{outputTypeSymbol.GetVariableName()}Map";

                    var entries = new List<MapEntryDatum>(outputTypeGroup.Count);
                    foreach (var argumentDatum in outputTypeGroup)
                    {
                        var mapKey = argumentDatum.LambdaBodyString;
                        var mapEntry = new MapEntryDatum(mapKey, argumentDatum.ExpressionChain);
                        entries.Add(mapEntry);
                    }

                    var map = new MapDatum(mapName, entries);
                    methodDatum = new SingleExpressionDictionaryImplMethodDatum(inputTypeName, outputTypeName, accessModifier, map);
                    break;
                }
            }

            return methodDatum;
        }

        public static bool GetExpression(GeneratorExecutionContext context, LambdaExpressionSyntax lambdaExpression, Compilation compilation, SemanticModel model, out ExpressionArgument expressionArgument)
        {
            var expressionChain = GetExpressionChain(context, lambdaExpression, model);
            var lambdaInputType = expressionChain[0].InputType;
            var lambdaOutputType = expressionChain[expressionChain.Count - 1].OutputType;
            var inputTypeAccess = lambdaInputType.GetVisibility();

            var containsPrivateOrProtectedMember =
                inputTypeAccess.IsPrivateOrProtected() ||
                ContainsPrivateOrProtectedMember(compilation, model, lambdaExpression);
            expressionArgument = new ExpressionArgument(lambdaExpression.Body.ToString(), expressionChain, lambdaInputType, lambdaOutputType, containsPrivateOrProtectedMember);

            return expressionChain != null;
        }

        public static MultiExpressionMethodDatum GetMultiExpression(IMethodSymbol methodSymbol)
        {
            var inputTypeSymbol = methodSymbol.TypeArguments[0];
            var inputTypeAccess = inputTypeSymbol.GetVisibility();
            var accessModifier = inputTypeAccess;

            for (int i = 1; i < methodSymbol.TypeArguments.Length; ++i)
            {
                var outputTypeSymbol = methodSymbol.TypeArguments[i];
                var outputTypeAccess = outputTypeSymbol.GetVisibility();
                if (outputTypeAccess < accessModifier || (accessModifier == Accessibility.Protected && outputTypeAccess == Accessibility.Internal))
                {
                    accessModifier = outputTypeAccess;
                }
            }

            var containsPrivateOrProtectedTypeArgument = accessModifier.IsPrivateOrProtected();
            var types = methodSymbol.TypeArguments;

            return new(accessModifier, types, containsPrivateOrProtectedTypeArgument);
        }

        public static List<ExpressionChain> GetExpressionChain(GeneratorExecutionContext context, LambdaExpressionSyntax lambdaExpression, SemanticModel model)
        {
            var members = new List<ExpressionChain>();
            var expression = lambdaExpression.ExpressionBody;
            var expressionChain = expression as MemberAccessExpressionSyntax;

            while (expressionChain != null)
            {
                var name = expressionChain.Name.ToString();
                var inputType = model.GetTypeInfo(expressionChain.ChildNodes().ElementAt(0)).Type as INamedTypeSymbol;
                var outputType = model.GetTypeInfo(expressionChain.ChildNodes().ElementAt(1)).Type as INamedTypeSymbol;

                members.Add(new(name, inputType, outputType));

                expression = expressionChain.Expression;
                expressionChain = expression as MemberAccessExpressionSyntax;
            }

            if (expression is not IdentifierNameSyntax firstLinkInChain)
            {
                // It stopped before reaching the lambda parameter, so the expression is invalid.
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        descriptor: DiagnosticWarnings.OnlyPropertyAndFieldAccessAllowed,
                        location: expression.GetLocation()));

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
                    descriptor: DiagnosticWarnings.LambdaParameterMustBeUsed,
                    location: lambdaExpression.Body.GetLocation()));

            return null;
        }
    }
}
