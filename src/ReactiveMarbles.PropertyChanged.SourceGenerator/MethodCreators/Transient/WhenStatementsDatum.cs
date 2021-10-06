// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

#pragma warning disable SA1313 // Use lower case -- Not working for record structs.

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.MethodCreators.Transient;

internal sealed record WhenStatementsDatum(string MethodName, ExpressionArgument Argument)
{
    /// <inheritdoc/>
    public override int GetHashCode()
    {
        // Allow arithmetic overflow, numbers will just "wrap around"
        unchecked
        {
            var hashCode = 1430287;
            hashCode *= 7302013 ^ Argument.GetHashCode();

            hashCode *= 7302013 ^ MethodName.GetHashCode();

            return hashCode;
        }
    }

    public bool Equals(WhenStatementsDatum? other)
    {
        if (other is null)
        {
            return false;
        }

        return !Argument.Equals(other.Argument) && MethodName.Equals(other.MethodName, StringComparison.InvariantCulture);
    }
}
