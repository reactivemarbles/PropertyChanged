// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    internal class BindGenerator : IGenerator
    {
        private static readonly RoslynBindExtensionCreator _bindExtensionCreator = new RoslynBindExtensionCreator();

        public IEnumerable<(string FileName, string SourceCode)> GenerateSourceFromInvocations(ITypeSymbol type, HashSet<InvocationInfo> invocations)
        {
            var publicInvocations = new List<BindInvocationInfo>();
            var privateInvocations = new List<BindInvocationInfo>();

            foreach (var invocation in invocations)
            {
                if (invocation is BindInvocationInfo bindInvocation)
                {
                    var list = bindInvocation.IsPublic ? publicInvocations : privateInvocations;
                    list.Add(bindInvocation);
                }
            }

            var extensionsSource = _bindExtensionCreator.Create(publicInvocations);

            if (!string.IsNullOrWhiteSpace(extensionsSource))
            {
                yield return ($"{type.ToDisplayString()}_Bind.extensions.g.cs", extensionsSource);
            }
        }

        private static MethodDatum CreateSingleExpressionMethodDatum(OutputTypeGroup outputTypeGroup)
        {
            MethodDatum methodDatum = null;

            var (_, expressionChain, inputTypeSymbol, outputTypeSymbol, _) = outputTypeGroup.ExpressionArguments[0];
            var (inputTypeName, outputTypeName) = (inputTypeSymbol.ToDisplayString(), outputTypeSymbol.ToDisplayString());

            var inputTypeAccess = inputTypeSymbol.GetAccessibility();
            var outputTypeAccess = outputTypeSymbol.GetOutputAccessibility();

            var accessModifier = inputTypeAccess;
            if (outputTypeAccess < inputTypeAccess || (inputTypeAccess == Accessibility.Protected && outputTypeAccess == Accessibility.Internal))
            {
                accessModifier = outputTypeAccess;
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
