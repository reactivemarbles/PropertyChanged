// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    internal static class WhenChangedGenerator
    {
        private const string ExtensionClassFullName = "NotifyPropertyChangedExtensions";
        private static readonly HashSet<MultiExpressionMethodDatum> EmptyMulti = new HashSet<MultiExpressionMethodDatum>();

        public static void GenerateWhenChanged(GeneratorExecutionContext context, Compilation compilation, SyntaxReceiver syntaxReceiver, bool useRoslyn)
        {
            var whenChangedInvocationInfo = ExtractWhenChangedInvocationInfo(context, compilation, syntaxReceiver);

            if (!whenChangedInvocationInfo.AllExpressionArgumentsAreValid)
            {
                return;
            }

            var partialClassData = CreateDatum(
                whenChangedInvocationInfo.PrivateExpressionArguments,
                whenChangedInvocationInfo.PrivateMultiExpressionMethodData,
                (inputTypeGroup, allMethodData) => new PartialClassDatum(inputTypeGroup.NamespaceName, inputTypeGroup.Name, inputTypeGroup.AccessModifier, inputTypeGroup.AncestorClasses, allMethodData));

            var extensionClassData = CreateDatum(
                whenChangedInvocationInfo.PublicExpressionArguments,
                whenChangedInvocationInfo.PublicMultiExpressionMethodData,
                (inputTypeGroup, allMethodData) => new ExtensionClassDatum(inputTypeGroup.Name, allMethodData));

            ISourceCreator extensionClassCreator = !useRoslyn ? new StringBuilderWhenChangedExtensionClassCreator() : new RoslynWhenChangedExtensionCreator();
            var extensionsSource = extensionClassCreator.Create(extensionClassData);

            if (!string.IsNullOrWhiteSpace(extensionsSource))
            {
                context.AddSource("WhenChanged.extensions.g.cs", SourceText.From(extensionsSource, Encoding.UTF8));
            }

            ISourceCreator partialClassCreator = !useRoslyn ? new StringBuilderWhenChangedPartialClassCreator() : new RoslynWhenChangedPartialClassCreator();
            var partialSource = partialClassCreator.Create(partialClassData);

            if (!string.IsNullOrWhiteSpace(partialSource))
            {
                context.AddSource("WhenChanged.partial.g.cs", SourceText.From(partialSource, Encoding.UTF8));
            }
        }

        private static List<T> CreateDatum<T>(SortedList<ITypeSymbol, HashSet<ExpressionArgument>> argumentGroupings, IReadOnlyDictionary<ITypeSymbol, HashSet<MultiExpressionMethodDatum>> methodGroupings, Func<InputTypeGroup, List<MethodDatum>, T> createFunc)
        {
            var datumData = new List<T>();

            foreach (var grouping in argumentGroupings)
            {
                if (!methodGroupings.TryGetValue(grouping.Key, out var multiMethods))
                {
                    multiMethods = EmptyMulti;
                }

                var arguments = new SortedList<ITypeSymbol, OutputTypeGroup>(TypeSymbolComparer.Default);

                foreach (var argument in grouping.Value)
                {
                    arguments.InsertOutputGroup(argument.OutputType, argument);
                }

                if (arguments.Count == 0)
                {
                    continue;
                }

                var inputGroup = arguments.Select(x => x.Value).ToInputTypeGroup(grouping.Key);

                var allMethodData = inputGroup.OutputTypeGroups.Select(CreateSingleExpressionMethodDatum).Concat(multiMethods).ToList();

                var datum = createFunc(inputGroup, allMethodData);

                datumData.Add(datum);
            }

            return datumData;
        }

        private static WhenChangedInvocationInfo ExtractWhenChangedInvocationInfo(GeneratorExecutionContext context, Compilation compilation, SyntaxReceiver syntaxReceiver)
        {
            var allExpressionArgumentsAreValid = true;
            var privateExpressionArguments = new SortedList<ITypeSymbol, HashSet<ExpressionArgument>>(TypeSymbolComparer.Default);
            var publicExpressionArguments = new SortedList<ITypeSymbol, HashSet<ExpressionArgument>>(TypeSymbolComparer.Default);
            var privateMultiExpressionMethodData = new SortedList<ITypeSymbol, HashSet<MultiExpressionMethodDatum>>(TypeSymbolComparer.Default);
            var publicMultiExpressionMethodData = new SortedList<ITypeSymbol, HashSet<MultiExpressionMethodDatum>>(TypeSymbolComparer.Default);

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
                        allExpressionArgumentsAreValid &= GeneratorHelpers.GetExpression(context, methodSymbol, lambdaExpression, compilation, model, out var expressionArgument);

                        var list = expressionArgument.ContainsPrivateOrProtectedMember ? privateExpressionArguments : publicExpressionArguments;

                        list.ListInsert(expressionArgument.InputType, expressionArgument);
                    }
                    else
                    {
                        // The argument is evaluates to an expression but it's not inline (could be a variable, method invocation, etc).
                        context.ReportDiagnostic(DiagnosticWarnings.ExpressionMustBeInline, argument.GetLocation());

                        allExpressionArgumentsAreValid = false;
                    }
                }

                if (methodSymbol.TypeArguments.Length > 2)
                {
                    var multiExpression = GeneratorHelpers.GetMultiExpression(methodSymbol);

                    var list = multiExpression.ContainsPrivateOrProtectedTypeArgument ? privateMultiExpressionMethodData : publicMultiExpressionMethodData;

                    list.ListInsert(multiExpression.InputType, multiExpression);
                }
            }

            return new WhenChangedInvocationInfo(allExpressionArgumentsAreValid, privateExpressionArguments, publicExpressionArguments, privateMultiExpressionMethodData, publicMultiExpressionMethodData);
        }

        private static MethodDatum CreateSingleExpressionMethodDatum(OutputTypeGroup outputTypeGroup)
        {
            MethodDatum methodDatum = null;

            var (_, expressionChain, inputTypeSymbol, outputTypeSymbol, _) = outputTypeGroup.ExpressionArguments[0];
            var (inputTypeName, outputTypeName) = (inputTypeSymbol.ToDisplayString(), outputTypeSymbol.ToDisplayString());

            var accessModifier = inputTypeSymbol.DeclaredAccessibility;
            if (outputTypeSymbol.DeclaredAccessibility < inputTypeSymbol.DeclaredAccessibility)
            {
                accessModifier = outputTypeSymbol.DeclaredAccessibility;
            }

            switch (outputTypeGroup.ExpressionArguments.Count)
            {
                case 1:
                    return new SingleExpressionOptimizedImplMethodDatum(inputTypeName, outputTypeName, accessModifier, expressionChain);
                case > 1:
                    {
                        var mapName = $"__generated{inputTypeSymbol.GetVariableName()}{outputTypeSymbol.GetVariableName()}Map";

                        var entries = new List<MapEntryDatum>(outputTypeGroup.ExpressionArguments.Count);
                        foreach (var argumentDatum in outputTypeGroup.ExpressionArguments)
                        {
                            var mapKey = argumentDatum.LambdaBodyString;
                            var mapEntry = new MapEntryDatum(mapKey, argumentDatum.ExpressionChain);
                            entries.Add(mapEntry);
                        }

                        var map = new MapDatum(mapName, entries);
                        return new SingleExpressionDictionaryImplMethodDatum(inputTypeName, outputTypeName, accessModifier, map);
                    }
            }

            return methodDatum;
        }
    }
}
