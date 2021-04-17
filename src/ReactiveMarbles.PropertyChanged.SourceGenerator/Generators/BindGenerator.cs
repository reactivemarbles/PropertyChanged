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
    }
}
