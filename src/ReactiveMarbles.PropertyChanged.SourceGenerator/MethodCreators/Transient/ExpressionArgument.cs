// Copyright (c) 2019-2023 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Microsoft.CodeAnalysis;

#pragma warning disable SA1313 // Use lower case -- Not working for record structs.

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.MethodCreators.Transient;

internal sealed record ExpressionArgument(string LambdaBodyString, IReadOnlyList<ExpressionChain> ExpressionChain, ITypeSymbol InputType, ITypeSymbol OutputType, bool ContainsPrivateOrProtectedMember) : IComparer<ExpressionArgument>
{
    public int Compare(ExpressionArgument x, ExpressionArgument y)
    {
        var inputCompare = TypeSymbolComparer.Default.Compare(x.InputType, y.InputType);

        return inputCompare != 0 ? inputCompare : TypeSymbolComparer.Default.Compare(x.OutputType, y.OutputType);
    }

    public bool Equals(ExpressionArgument? other) => other is not null &&
                                                     TypeSymbolComparer.Default.Equals(InputType, other.InputType) &&
                                                     TypeSymbolComparer.Default.Equals(OutputType, other.OutputType);

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = 1230885993;
            hashCode = (hashCode * -1521134295) + TypeSymbolComparer.Default.GetHashCode(InputType);
            hashCode = (hashCode * -1521134295) + TypeSymbolComparer.Default.GetHashCode(OutputType);

            return hashCode;
        }
    }
}
