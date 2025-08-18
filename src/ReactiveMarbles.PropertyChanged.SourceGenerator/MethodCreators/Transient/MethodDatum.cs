// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using ReactiveMarbles.PropertyChanged.SourceGenerator.Comparers;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.MethodCreators.Transient;

internal record MethodDatum(Accessibility ClassAccessibility, MethodDeclarationSyntax Expression)
{
    /// <inheritdoc/>
    public override int GetHashCode()
    {
        // Allow arithmetic overflow, numbers will just "wrap around"
        unchecked
        {
            var hashCode = 1430287;

            hashCode *= 7302013 ^ ClassAccessibility.GetHashCode();
            hashCode *= 7302013 ^ SyntaxNodeComparer.Default.GetHashCode(Expression);

            return hashCode;
        }
    }

    public virtual bool Equals(MethodDatum? other)
    {
        if (other is null)
        {
            return false;
        }

        if (!SyntaxNodeComparer.Default.Equals(other.Expression, Expression))
        {
            return false;
        }

        return ClassAccessibility == other.ClassAccessibility;
    }
}
