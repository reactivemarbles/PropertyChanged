// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using ReactiveMarbles.PropertyChanged.SourceGenerator.Comparers;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.MethodCreators
{
    internal record MethodDatum(bool IsExtension, string NamespaceName, string ClassName, Accessibility AccessibilityModifier, SemanticModel SemanticModel, MethodDeclarationSyntax Expression)
    {
        /// <inheritdoc/>
        public override int GetHashCode()
        {
            // Allow arithmetic overflow, numbers will just "wrap around"
            unchecked
            {
                var hashCode = 1430287;

                hashCode *= 7302013 ^ IsExtension.GetHashCode();
                hashCode *= 7302013 ^ StringComparer.InvariantCulture.GetHashCode(ClassName);
                hashCode *= 7302013 ^ StringComparer.InvariantCulture.GetHashCode(NamespaceName);
                hashCode *= 7302013 ^ SyntaxNodeComparer.Default.GetHashCode(Expression);
                hashCode *= 7302013 ^ SemanticModel.GetHashCode();
                hashCode *= 7302013 ^ AccessibilityModifier.GetHashCode();

                return hashCode;
            }
        }

        public virtual bool Equals(MethodDatum? other)
        {
            if (other is null)
            {
                return false;
            }

            if (!StringComparer.InvariantCulture.Equals(other.ClassName, ClassName))
            {
                return false;
            }

            if (!StringComparer.InvariantCulture.Equals(other.NamespaceName, NamespaceName))
            {
                return false;
            }

            if (IsExtension != other.IsExtension)
            {
                return false;
            }

            if (SemanticModel != other.SemanticModel)
            {
                return false;
            }

            if (AccessibilityModifier != other.AccessibilityModifier)
            {
                return false;
            }

            if (!SyntaxNodeComparer.Default.Equals(other.Expression, Expression))
            {
                return false;
            }

            return true;
        }
    }
}
