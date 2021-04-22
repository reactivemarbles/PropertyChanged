// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    internal class WhenChangedGenerator : IGenerator
    {
        private ISourceCreator _extensionClassCreator;
        private ISourceCreator _partialClassCreator;

        public WhenChangedGenerator(bool useRoslyn)
        {
            _extensionClassCreator = !useRoslyn ? new StringBuilderWhenChangedExtensionClassCreator() : new RoslynWhenChangedExtensionCreator();
            _partialClassCreator = !useRoslyn ? new StringBuilderWhenChangedPartialClassCreator() : new RoslynWhenChangedPartialClassCreator();
        }

        public IEnumerable<(string FileName, string SourceCode)> GenerateSourceFromInvocations(ITypeSymbol type, HashSet<InvocationInfo> invocations)
        {
            var privateExpressions = new HashSet<ExpressionArgument>();
            var publicExpressions = new HashSet<ExpressionArgument>();
            var privateMultiItems = new HashSet<MultiExpressionMethodDatum>();
            var publicMultiItems = new HashSet<MultiExpressionMethodDatum>();
            foreach (var invocation in invocations)
            {
                if (invocation is WhenChangedExpressionInvocationInfo whenChangedExpressionInvocationInfo)
                {
                    var list = whenChangedExpressionInvocationInfo.IsPublic ? publicExpressions : privateExpressions;
                    list.Add(whenChangedExpressionInvocationInfo.ExpressionArgument);
                }

                if (invocation is WhenChangedMultiMethodInvocationInfo multiInvocationInfo)
                {
                    if (multiInvocationInfo.IsPublic)
                    {
                        publicMultiItems.Add(multiInvocationInfo.MultiExpression);
                    }

                    privateMultiItems.Add(multiInvocationInfo.MultiExpression);
                }
            }

            var partialClassData = CreateDatum(
                type,
                privateExpressions,
                privateMultiItems,
                (inputTypeGroup, allMethodData) => new PartialClassDatum(inputTypeGroup.NamespaceName, inputTypeGroup.Name, inputTypeGroup.AccessModifier, inputTypeGroup.AncestorClasses, allMethodData));

            var extensionClassData = CreateDatum(
                type,
                publicExpressions,
                publicMultiItems,
                (inputTypeGroup, allMethodData) => new ExtensionClassDatum(inputTypeGroup.Name, allMethodData));

            var extensionsSource = _extensionClassCreator.Create(extensionClassData);

            if (!string.IsNullOrWhiteSpace(extensionsSource))
            {
                yield return ($"{type.ToDisplayString()}_WhenChanged.extensions.g.cs", extensionsSource);
            }

            var partialSource = _partialClassCreator.Create(partialClassData);

            if (!string.IsNullOrWhiteSpace(partialSource))
            {
                yield return ($"{type.ToDisplayString()}_WhenChanged.partial.g.cs", partialSource);
            }
        }

        private static List<T> CreateDatum<T>(ITypeSymbol type, IEnumerable<ExpressionArgument> expressionArguments, IEnumerable<MultiExpressionMethodDatum> multiMethods, Func<InputTypeGroup, List<MethodDatum>, T> createFunc)
        {
            var datumData = new List<T>();

            var arguments = new SortedList<ITypeSymbol, OutputTypeGroup>(TypeSymbolComparer.Default);

            foreach (var argument in expressionArguments)
            {
                arguments.InsertOutputGroup(argument.OutputType, argument);
            }

            if (arguments.Count == 0)
            {
                return datumData;
            }

            var inputGroup = arguments.Select(x => x.Value).ToInputTypeGroup(type);

            var allMethodData = inputGroup.OutputTypeGroups.Select(CreateSingleExpressionMethodDatum).Concat(multiMethods).ToList();

            var datum = createFunc(inputGroup, allMethodData);

            datumData.Add(datum);

            return datumData;
        }

        private static MethodDatum CreateSingleExpressionMethodDatum(OutputTypeGroup outputTypeGroup)
        {
            MethodDatum methodDatum = null;

            var (_, expressionChain, inputTypeSymbol, outputTypeSymbol, _) = outputTypeGroup.ExpressionArguments[0];
            var (inputTypeName, outputTypeName) = (inputTypeSymbol.ToDisplayString(), outputTypeSymbol.ToDisplayString());

            var inputTypeAccess = inputTypeSymbol.GetVisibility();
            var outputTypeAccess = outputTypeSymbol.GetVisibility();

            var accessModifier = inputTypeAccess;
            if (outputTypeAccess < inputTypeAccess || (inputTypeAccess == Accessibility.Protected && outputTypeAccess == Accessibility.Internal))
            {
                accessModifier = outputTypeAccess;
            }

            if (inputTypeAccess == Accessibility.Protected || outputTypeAccess == Accessibility.Protected)
            {
                accessModifier = Accessibility.Internal;
            }

            if (inputTypeAccess < Accessibility.Public && outputTypeAccess < Accessibility.Public)
            {
                accessModifier = Accessibility.Public;
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
