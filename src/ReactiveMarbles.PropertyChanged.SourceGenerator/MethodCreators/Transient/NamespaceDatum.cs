// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.MethodCreators.Transient;

internal sealed record NamespaceDatum(string NamespaceName, bool IsGlobal)
{
    public Dictionary<string, ClassDatum> ClassDictionary { get; } = [];

    public IEnumerable<ClassDatum> Classes => ClassDictionary.Values;

    public bool Equals(NamespaceDatum? other) => other?.NamespaceName.Equals(NamespaceName, StringComparison.InvariantCulture) == true;

    public override int GetHashCode() => NamespaceName.GetHashCode();
}
