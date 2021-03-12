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

        public InputTypeGroup(ITypeSymbol inputType, List<OutputTypeGroup> outputTypeGroups)
        {
            _inputType = inputType;
            OutputTypeGroups = outputTypeGroups;

            var containingNamespace = _inputType.ContainingNamespace;
            NamespaceName = string.IsNullOrEmpty(containingNamespace?.Name) ? string.Empty : containingNamespace.ToDisplayString();

            var ancestorClasses = new List<AncestorClassInfo>();
            var containingType = _inputType.ContainingType;
            while (containingType != null)
            {
                ancestorClasses.Add(new(containingType.Name, containingType.DeclaredAccessibility));
                containingType = containingType.ContainingType;
            }

            AncestorClasses = ancestorClasses;
        }

        public string NamespaceName { get; }

        public string Name => _inputType.Name;

        public string FullName => _inputType.ToDisplayString();

        public List<AncestorClassInfo> AncestorClasses { get; }

        public Accessibility AccessModifier => _inputType.DeclaredAccessibility;

        public List<OutputTypeGroup> OutputTypeGroups { get; }
    }
}
