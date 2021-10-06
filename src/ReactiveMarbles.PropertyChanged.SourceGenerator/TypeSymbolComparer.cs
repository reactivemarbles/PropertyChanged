// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator;

internal class TypeSymbolComparer : IComparer<ITypeSymbol>, IEqualityComparer<ITypeSymbol>
{
    public static TypeSymbolComparer Default { get; } = new();

    public int Compare(ITypeSymbol x, ITypeSymbol y)
    {
        if (ReferenceEquals(x, y))
        {
            return 0;
        }

        if (x is INamedTypeSymbol xNamed && y is INamedTypeSymbol yNamed)
        {
            return string.CompareOrdinal(xNamed.ToDisplayString(), yNamed.ToDisplayString());
        }

        return string.CompareOrdinal(x.Name, y.Name);
    }

    public bool Equals(ITypeSymbol? x, ITypeSymbol? y)
    {
        if (x is null && y is null)
        {
            return true;
        }

        if (x is null || y is null)
        {
            return false;
        }

        return x.ToDisplayString().Equals(y.ToDisplayString(), StringComparison.InvariantCulture);
    }

    public int GetHashCode(ITypeSymbol obj) => obj.ToDisplayString().GetHashCode();
}
