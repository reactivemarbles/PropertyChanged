// Copyright (c) 2019-2023 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

#pragma warning disable SA1313 // Use lower case -- Not working for record structs.

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.MethodCreators.Transient;

internal sealed record ClassDatum(string ClassName, Accessibility AccessibilityModifier, bool IsExtension, IReadOnlyList<ClassDatum> Ancestors)
{
    public HashSet<MethodDatum> MethodData { get; } = new();

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        // Allow arithmetic overflow, numbers will just "wrap around"
        unchecked
        {
            var hashCode = 1430287;

            hashCode *= 7302013 ^ IsExtension.GetHashCode();
            hashCode *= 7302013 ^ StringComparer.InvariantCulture.GetHashCode(ClassName);
            hashCode *= 7302013 ^ AccessibilityModifier.GetHashCode();

            return hashCode;
        }
    }

    public bool Equals(ClassDatum? other)
    {
        if (other is null)
        {
            return false;
        }

        if (!StringComparer.InvariantCulture.Equals(other.ClassName, ClassName))
        {
            return false;
        }

        if (IsExtension != other.IsExtension)
        {
            return false;
        }

        return AccessibilityModifier == other.AccessibilityModifier;
    }
}
