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
        private readonly Dictionary<Type, (RoslynBindBase Extractor, List<BindInvocationInfo> BindInfoList, string Name)> _extractors = new()
        {
            [typeof(ExtensionBindInvocationInfo)] = (new RoslynBindExtensionCreator(), new List<BindInvocationInfo>(), "TwoWayExtensions"),
            [typeof(PartialBindInvocationInfo)] = (new RoslynBindPartialClassCreator(), new List<BindInvocationInfo>(), "TwoWayPartial"),
            [typeof(ExtensionOneWayBindInvocationInfo)] = (new RoslynOneWayBindExtensionCreator(), new List<BindInvocationInfo>(), "OneWayExtensions"),
            [typeof(RoslynOneWayBindPartialClassCreator)] = (new RoslynOneWayBindPartialClassCreator(), new List<BindInvocationInfo>(), "OneWayPartial"),
        };

        public IEnumerable<(string FileName, string SourceCode)> GenerateSourceFromInvocations(ITypeSymbol type, HashSet<InvocationInfo> invocations)
        {
            foreach (var invocation in invocations.OfType<BindInvocationInfo>())
            {
                var (_, bindInfoList, _) = _extractors[invocation.GetType()];

                bindInfoList.Add(invocation);
            }

            foreach (var extractorType in _extractors)
            {
                var (extractor, bindInfoList, name) = extractorType.Value;

                var value = Generate(type, bindInfoList, name, extractor);

                if (value != null)
                {
                    yield return value.Value;
                }
            }
        }

        private (string FileName, string SourceCode)? Generate(ITypeSymbol type, IReadOnlyList<BindInvocationInfo> bindingInvocations, string bindType, ISourceCreator generator)
        {
            if (bindingInvocations.Count == 0)
            {
                return default;
            }

            var extensionsSource = generator.Create(bindingInvocations);

            return !string.IsNullOrWhiteSpace(extensionsSource) ?
                ($"{type.ToDisplayString()}_{bindType}.extensions.g.cs", extensionsSource) :
                default;
        }
    }
}
