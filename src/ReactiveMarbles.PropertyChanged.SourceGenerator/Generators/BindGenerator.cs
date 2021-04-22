// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Microsoft.CodeAnalysis;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    internal class BindGenerator : IGenerator
    {
        private static readonly RoslynBindExtensionCreator _bindExtensionCreator = new RoslynBindExtensionCreator();
        private static readonly RoslynBindPartialClassCreator _bindPartialCreator = new RoslynBindPartialClassCreator();

        public IEnumerable<(string FileName, string SourceCode)> GenerateSourceFromInvocations(ITypeSymbol type, HashSet<InvocationInfo> invocations)
        {
            var publicInvocations = new List<ExtensionBindInvocationInfo>();
            var privateInvocations = new List<PartialBindInvocationInfo>();

            foreach (var invocation in invocations)
            {
                if (invocation is ExtensionBindInvocationInfo bindInvocation)
                {
                    publicInvocations.Add(bindInvocation);
                }

                if (invocation is PartialBindInvocationInfo partialBindInvocation)
                {
                    privateInvocations.Add(partialBindInvocation);
                }
            }

            var extensionsSource = _bindExtensionCreator.Create(publicInvocations);

            if (!string.IsNullOrWhiteSpace(extensionsSource))
            {
                yield return ($"{type.ToDisplayString()}_Bind.extensions.g.cs", extensionsSource);
            }

            var partialSource = _bindPartialCreator.Create(privateInvocations);

            if (!string.IsNullOrWhiteSpace(partialSource))
            {
                yield return ($"{type.ToDisplayString()}_Bind.partial.g.cs", partialSource);
            }
        }
    }
}
