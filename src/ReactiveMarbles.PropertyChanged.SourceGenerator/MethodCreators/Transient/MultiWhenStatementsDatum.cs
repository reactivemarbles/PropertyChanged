// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.MethodCreators.Transient;

internal record MultiWhenStatementsDatum(string MethodName, IReadOnlyList<WhenStatementsDatum> Arguments, bool IsExtensionMethod, Accessibility ClassAccessibility, ITypeSymbol InputType, ITypeSymbol OutputType, IReadOnlyList<ITypeSymbol> TypeArguments, ISymbol CallingSymbol)
{
    public override int GetHashCode()
    {
        // Allow arithmetic overflow, numbers will just "wrap around"
        unchecked
        {
            var hashCode = Arguments.Aggregate(1430287, (current, argument) => current * (7302013 ^ argument.GetHashCode()));

            hashCode *= 7302013 ^ MethodName.GetHashCode();
            hashCode *= 7302013 ^ ClassAccessibility.GetHashCode();
            hashCode *= 7302013 ^ IsExtensionMethod.GetHashCode();

            return hashCode;
        }
    }

    public virtual bool Equals(MultiWhenStatementsDatum? other)
    {
        if (other is null)
        {
            return false;
        }

        if (other.Arguments.Count != Arguments.Count)
        {
            return false;
        }

        if (!other.MethodName.Equals(MethodName, StringComparison.InvariantCulture))
        {
            return false;
        }

        if (Arguments.Where((t, i) => !t.Equals(other.Arguments[i])).Any())
        {
            return false;
        }

        if (IsExtensionMethod != other.IsExtensionMethod)
        {
            return false;
        }

        return ClassAccessibility == other.ClassAccessibility;
    }
}
