// Copyright (c) 2019-2020 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    [Generator]
    internal sealed class WhenChangedGenerator : ISourceGenerator
    {
        internal static readonly DiagnosticDescriptor ExpressionMustBeInline = new DiagnosticDescriptor(
            id: "RXM001",
            title: "Expression chain must be inline",
            messageFormat: "The expression must be inline (e.g. not a variable or method invocation).",
            category: "Compiler",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        internal static readonly DiagnosticDescriptor OnlyPropertyAndFieldAccessAllowed = new DiagnosticDescriptor(
            id: "RXM002",
            title: "Expression chain may only consist of property and field access",
            messageFormat: "The expression may only consist of field and property access",
            category: "Compiler",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        internal static readonly DiagnosticDescriptor LambdaParameterMustBeUsed = new DiagnosticDescriptor(
            id: "RXM003",
            title: "Lambda parameter must be used in expression chain",
            messageFormat: "The lambda parameter must be used in the expression chain",
            category: "Compiler",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        private const string ExtensionClassFullName = "NotifyPropertyChangedExtensions";

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            CSharpParseOptions options = (context.Compilation as CSharpCompilation).SyntaxTrees[0].Options as CSharpParseOptions;
            var stubSource = StringBuilderSourceCreatorHelper.GetWhenChangedStubClass();
            Compilation compilation = context.Compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(SourceText.From(stubSource, Encoding.UTF8), options));
            context.AddSource($"WhenChanged.Stubs.g.cs", SourceText.From(stubSource, Encoding.UTF8));

            if (context.SyntaxReceiver is not SyntaxReceiver syntaxReceiver)
            {
                return;
            }

            WhenChangedInvocationInfo whenChangedInvocationInfo = ExtractWhenChangedInvocationInfo(context, compilation, syntaxReceiver);

            if (!whenChangedInvocationInfo.AllExpressionArgumentsAreValid)
            {
                return;
            }

            var partialClassData = whenChangedInvocationInfo.ExpressionArguments
                .Where(x => x.ContainsPrivateOrProtectedMember)
                .GroupBy(x => x.InputType)
                .Select(x => x
                    .GroupBy(y => y.OutputType)
                    .Select(y => y.ToOuputTypeGroup())
                    .ToInputTypeGroup(x.Key))
                .GroupJoin(
                    whenChangedInvocationInfo.MultiExpressionMethodData,
                    x => x.FullName,
                    x => x.InputTypeFullName,
                    (inputTypeGroup, multiExpressionMethodData) =>
                    {
                        var allMethodData = inputTypeGroup
                            .OutputTypeGroups
                            .Select(CreateSingleExpressionMethodDatum)
                            .Concat(multiExpressionMethodData.Where(x => x.ContainsPrivateOrProtectedTypeArgument));

                        return new PartialClassDatum(inputTypeGroup.NamespaceName, inputTypeGroup.Name, inputTypeGroup.AccessModifier, inputTypeGroup.AncestorClasses, allMethodData);
                    })
                .ToList();

            var extensionClassData = whenChangedInvocationInfo.ExpressionArguments
                .Where(x => !x.ContainsPrivateOrProtectedMember)
                .GroupBy(x => x.InputType)
                .Select(x => x
                    .GroupBy(y => y.OutputType)
                    .Select(y => y.ToOuputTypeGroup())
                    .ToInputTypeGroup(x.Key))
                .GroupJoin(
                    whenChangedInvocationInfo.MultiExpressionMethodData,
                    x => x.FullName,
                    x => x.InputTypeFullName,
                    (inputTypeGroup, multiExpressionMethodData) =>
                    {
                        var allMethodData = inputTypeGroup
                            .OutputTypeGroups
                            .Select(CreateSingleExpressionMethodDatum)
                            .Concat(multiExpressionMethodData.Where(x => !x.ContainsPrivateOrProtectedTypeArgument));

                        return new ExtensionClassDatum(inputTypeGroup.Name, allMethodData);
                    })
                .ToList();

            var extensionClassCreator = new StringBuilderExtensionClassCreator();
            for (int i = 0; i < extensionClassData.Count; i++)
            {
                var source = extensionClassCreator.Create(extensionClassData[i]);
                context.AddSource($"WhenChanged.{extensionClassData[i].Name}{i.ToString()}.g.cs", SourceText.From(source, Encoding.UTF8));
            }

            var partialClassCreator = new StringBuilderPartialClassCreator();
            for (int i = 0; i < partialClassData.Count; i++)
            {
                var source = partialClassCreator.Create(partialClassData[i]);
                context.AddSource($"{partialClassData[i].Name}{i.ToString()}.WhenChanged.g.cs", SourceText.From(source, Encoding.UTF8));
            }
        }

        private static WhenChangedInvocationInfo ExtractWhenChangedInvocationInfo(GeneratorExecutionContext context, Compilation compilation, SyntaxReceiver syntaxReceiver)
        {
            var allExpressionArgumentsAreValid = true;
            var expressionArguments = new HashSet<ExpressionArgument>();
            var multiExpressionMethodData = new HashSet<MultiExpressionMethodDatum>();

            foreach (var invocationExpression in syntaxReceiver.WhenChangedMethods)
            {
                var model = compilation.GetSemanticModel(invocationExpression.SyntaxTree);
                var symbol = model.GetSymbolInfo(invocationExpression).Symbol;

                if (symbol is IMethodSymbol methodSymbol)
                {
                    if (!methodSymbol.ContainingType.ToDisplayString().Equals(ExtensionClassFullName))
                    {
                        continue;
                    }

                    foreach (var argument in invocationExpression.ArgumentList.Arguments)
                    {
                        if (model.GetTypeInfo(argument.Expression).ConvertedType.Name.Equals("Expression"))
                        {
                            if (argument.Expression is LambdaExpressionSyntax lambdaExpression)
                            {
                                var lambdaInputType = methodSymbol.TypeArguments[0];
                                var lambdaOutputType = model.GetTypeInfo(lambdaExpression.Body).Type;
                                var expressionChain = GetExpressionChain(context, lambdaExpression);

                                var containsPrivateOrProtectedMember =
                                    lambdaInputType.DeclaredAccessibility <= Microsoft.CodeAnalysis.Accessibility.Protected ||
                                    ContainsPrivateOrProtectedMember(compilation, model, lambdaExpression);
                                expressionArguments.Add(new(lambdaExpression.Body.ToString(), expressionChain, lambdaInputType, lambdaOutputType, containsPrivateOrProtectedMember));
                                allExpressionArgumentsAreValid &= expressionChain != null;
                            }
                            else
                            {
                                // The argument is evaluates to an expression but it's not inline (could be a variable, method invocation, etc).
                                context.ReportDiagnostic(
                                    Diagnostic.Create(
                                        descriptor: ExpressionMustBeInline,
                                        location: argument.GetLocation()));

                                allExpressionArgumentsAreValid = false;
                            }
                        }
                    }

                    if (methodSymbol.TypeArguments.Length > 2)
                    {
                        var minAccessibility = methodSymbol.TypeArguments.Min(x => x.DeclaredAccessibility);
                        var accessModifier = minAccessibility.ToString().ToLower();
                        var containsPrivateOrProtectedTypeArgument = minAccessibility <= Microsoft.CodeAnalysis.Accessibility.Protected;
                        var typeNames = methodSymbol.TypeArguments.Select(x => x.ToDisplayString());
                        multiExpressionMethodData.Add(new(accessModifier, typeNames, containsPrivateOrProtectedTypeArgument));
                    }
                }
            }

            return new WhenChangedInvocationInfo(allExpressionArgumentsAreValid, expressionArguments, multiExpressionMethodData);
        }

        private static MethodDatum CreateSingleExpressionMethodDatum(OutputTypeGroup outputTypeGroup)
        {
            MethodDatum methodDatum = null;

            var (lambdaBodyString, expressionChain, inputTypeSymbol, outputTypeSymbol, containsPrivateOrProtectedMember) = outputTypeGroup.ExpressionArguments.First();
            var (inputTypeName, outputTypeName) = (inputTypeSymbol.ToDisplayString(), outputTypeSymbol.ToDisplayString());

            var accessModifier = inputTypeSymbol.DeclaredAccessibility;
            if (outputTypeSymbol.DeclaredAccessibility < inputTypeSymbol.DeclaredAccessibility)
            {
                accessModifier = outputTypeSymbol.DeclaredAccessibility;
            }

            var accessModifierName = accessModifier.ToString().ToLower();

            if (outputTypeGroup.ExpressionArguments.Count == 1)
            {
                methodDatum = new SingleExpressionOptimizedImplMethodDatum(inputTypeName, outputTypeName, accessModifierName, expressionChain);
            }
            else if (outputTypeGroup.ExpressionArguments.Count > 1)
            {
                var mapName = $"{outputTypeSymbol.ToDisplayParts().Where(x => x.Kind != SymbolDisplayPartKind.Punctuation).Select(x => x.ToString()).Aggregate((a, b) => a + b)}Map";

                var entries = new List<MapEntryDatum>(outputTypeGroup.ExpressionArguments.Count);
                foreach (var argumentDatum in outputTypeGroup.ExpressionArguments)
                {
                    var mapKey = argumentDatum.LambdaBodyString;
                    var mapEntry = new MapEntryDatum(mapKey, argumentDatum.ExpressionChain);
                    entries.Add(mapEntry);
                }

                var map = new MapDatum(mapName, entries);
                methodDatum = new SingleExpressionDictionaryImplMethodDatum(inputTypeName, outputTypeName, accessModifierName, map);
            }

            return methodDatum;
        }

        private static List<string> GetExpressionChain(GeneratorExecutionContext context, LambdaExpressionSyntax lambdaExpression)
        {
            var members = new List<string>();
            var expression = lambdaExpression.ExpressionBody;
            var expressionChain = expression as MemberAccessExpressionSyntax;

            while (expressionChain != null)
            {
                members.Add(expressionChain.Name.ToString());
                expression = expressionChain.Expression;
                expressionChain = expression as MemberAccessExpressionSyntax;
            }

            if (expression is not IdentifierNameSyntax firstLinkInChain)
            {
                // It stopped before reaching the lambda parameter, so the expression is invalid.
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        descriptor: OnlyPropertyAndFieldAccessAllowed,
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
                    descriptor: LambdaParameterMustBeUsed,
                    location: lambdaExpression.Body.GetLocation()));

            return null;
        }

        private static bool ContainsPrivateOrProtectedMember(Compilation compilation, SemanticModel model, LambdaExpressionSyntax lambdaExpression)
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

            for (int i = members.Count - 1; i > 0; i--)
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

                if (propertyAccessibility <= Microsoft.CodeAnalysis.Accessibility.Protected)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
