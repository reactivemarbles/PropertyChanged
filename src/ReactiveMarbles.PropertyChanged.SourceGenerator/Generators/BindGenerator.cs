// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    internal class BindGenerator : IGenerator
    {
        public IEnumerable<(string FileName, string SourceCode)> GenerateSourceFromInvocations(ITypeSymbol type, HashSet<TypeDatum> invocations)
        {
            var extractors = new Dictionary<Type, (RoslynBindBase Extractor, List<BindInvocationInfo> List, string Name)>
            {
                [typeof(ExtensionBindInvocationInfo)] = (new RoslynBindExtensionCreator(), new(), "TwoWayExtensions"),
                [typeof(PartialBindInvocationInfo)] = (new RoslynBindPartialClassCreator(), new(), "TwoWayPartial"),
                [typeof(ExtensionOneWayBindInvocationInfo)] = (new RoslynOneWayBindExtensionCreator(), new(), "OneWayExtensions"),
                [typeof(PartialOneWayBindInvocationInfo)] = (new RoslynOneWayBindPartialClassCreator(), new(), "OneWayPartial"),
            };

            foreach (var invocation in invocations.OfType<BindInvocationInfo>())
            {
                var (_, bindInfoList, _) = extractors[invocation.GetType()];

                bindInfoList.Add(invocation);
            }

            foreach (var extractorType in extractors)
            {
                var (extractor, bindInfoList, name) = extractorType.Value;

                var value = Generate(type, bindInfoList, name, extractor);

                if (value != null)
                {
                    yield return value.Value;
                }
            }
        }

        private static (string FileName, string SourceCode)? Generate(ISymbol type, IReadOnlyCollection<BindInvocationInfo> bindingInvocations, string bindType, ISourceCreator generator)
        {
            if (bindingInvocations.Count == 0)
            {
                return default;
            }

            var extensionsSource = generator.Create(bindingInvocations);

            if (extensionsSource is null)
            {
                return default;
            }

            return !string.IsNullOrWhiteSpace(extensionsSource) ?
                ($"{type.ToDisplayString()}_{bindType}.extensions.g.cs", extensionsSource) :
                default;
        }
    }
}
