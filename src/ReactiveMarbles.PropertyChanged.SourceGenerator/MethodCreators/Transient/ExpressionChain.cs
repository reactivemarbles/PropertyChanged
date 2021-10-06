// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;

#pragma warning disable SA1313 // Use lower case -- Not working for record structs.

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.MethodCreators.Transient;

internal sealed record ExpressionChain(string Name, ITypeSymbol InputType, ITypeSymbol OutputType)
{
    public bool Equals(ExpressionChain? other)
    {
        if (other is null)
        {
            return false;
        }

        if (!string.Equals(Name, other.Name, System.StringComparison.InvariantCulture))
        {
            return false;
        }

        return TypeSymbolComparer.Default.Equals(InputType, other.InputType) && TypeSymbolComparer.Default.Equals(OutputType, other.OutputType);
    }

    public override int GetHashCode()
    {
        // Allow arithmetic overflow, numbers will just "wrap around"
        unchecked
        {
            var hashCode = 1430287;

            hashCode *= 7302013 ^ Name.GetHashCode();
            hashCode *= 7302013 ^ TypeSymbolComparer.Default.GetHashCode(InputType);
            hashCode *= 7302013 ^ TypeSymbolComparer.Default.GetHashCode(OutputType);

            return hashCode;
        }
    }
}
