// Copyright (c) 2019-2020 ReactiveUI Association Incorporated. All rights reserved.
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
        }

        public string NamespaceName => _inputType.ContainingNamespace?.ToDisplayString();

        public string Name => _inputType.Name;

        public string FullName => _inputType.ToDisplayString();

        public IEnumerable<OutputTypeGroup> OutputTypeGroups { get; }
    }
}
