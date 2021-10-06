// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using Microsoft.CodeAnalysis;

#pragma warning disable SA1313 // Use lower case -- Not working for record structs.

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.MethodCreators.Transient;

internal sealed record BindStatementsDatum(ExpressionArgument HostArgument, ExpressionArgument TargetArgument, string MethodName, bool HasConverters, Accessibility ClassAccessibilty, Accessibility MethodAccessibility, SemanticModel Model)
{
    /// <inheritdoc/>
    public override int GetHashCode()
    {
        // Allow arithmetic overflow, numbers will just "wrap around"
        unchecked
        {
            var hashCode = 1430287;

            hashCode *= 7302013 ^ HasConverters.GetHashCode();
            hashCode *= 7302013 ^ MethodAccessibility.GetHashCode();
            hashCode *= 7302013 ^ HostArgument.GetHashCode();
            hashCode *= 7302013 ^ TargetArgument.GetHashCode();
            hashCode *= 7302013 ^ MethodName.GetHashCode();

            return hashCode;
        }
    }

    public bool Equals(BindStatementsDatum? other)
    {
        if (other is null)
        {
            return false;
        }

        if (HasConverters != other.HasConverters)
        {
            return false;
        }

        if (MethodAccessibility != other.MethodAccessibility)
        {
            return false;
        }

        if (!HostArgument.Equals(other.HostArgument))
        {
            return false;
        }

        return TargetArgument.Equals(other.TargetArgument) && MethodName.Equals(other.MethodName, StringComparison.InvariantCulture);
    }

    public override string ToString() =>
        $"{MethodAccessibility.ToString()} IObservable<{TargetArgument.OutputType}> {MethodName}<{HostArgument.InputType}, {TargetArgument.OutputType}>(this {TargetArgument.InputType} input, {HostArgument.LambdaBodyString} inputExpression, {TargetArgument.LambdaBodyString} outputExpression)";
}
