// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    internal readonly struct InputTypeGroup
    {
        private readonly ITypeSymbol _inputType;

        public InputTypeGroup(ITypeSymbol inputType, IEnumerable<OutputTypeGroup> outputTypeGroups)
        {
            _inputType = inputType;
            OutputTypeGroups = outputTypeGroups;

            var containingNamespace = _inputType.ContainingNamespace;
            if (string.IsNullOrEmpty(containingNamespace?.Name))
            {
                NamespaceName = string.Empty;
            }
            else
            {
                NamespaceName = containingNamespace.ToDisplayString();
            }

            var ancestorClasses = new List<AncestorClassInfo>();
            var containingType = _inputType.ContainingType;
            while (containingType != null)
            {
                ancestorClasses.Add(new(containingType.Name, containingType.DeclaredAccessibility.ToString().ToLower()));
                containingType = containingType.ContainingType;
            }

            AncestorClasses = ancestorClasses;
        }

        public string NamespaceName { get; }

        public string Name => _inputType.Name;

        public string FullName => _inputType.ToDisplayString();

        public IEnumerable<AncestorClassInfo> AncestorClasses { get; }

        public string AccessModifier => _inputType.DeclaredAccessibility.ToString().ToLower();

        public IEnumerable<OutputTypeGroup> OutputTypeGroups { get; }
    }
}
