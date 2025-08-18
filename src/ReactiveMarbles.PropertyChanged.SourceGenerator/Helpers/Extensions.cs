// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;

using ReactiveMarbles.PropertyChanged.SourceGenerator.MethodCreators.Transient;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Helpers;

internal static class Extensions
{
    public static List<ClassDatum> GetAncestorsClassDatum(this ITypeSymbol inputType) =>
        [.. GetAncestors(inputType).Select(x => new ClassDatum(x.Name, x.DeclaredAccessibility, false, []))];

    public static IEnumerable<ITypeSymbol> GetThisAndAncestors(this ITypeSymbol inputType)
    {
        var containingType = inputType;

        while (containingType is not null)
        {
            yield return containingType;
            containingType = containingType.ContainingType;
        }
    }

    public static IEnumerable<ITypeSymbol> GetAncestors(this ITypeSymbol inputType)
    {
        var containingType = inputType.ContainingType;

        while (containingType is not null)
        {
            yield return containingType;
            containingType = containingType.ContainingType;
        }
    }

    public static void ReportDiagnostic(this in GeneratorExecutionContext context, DiagnosticDescriptor descriptor, Location? location = null, params object[] items) => context.ReportDiagnostic(Diagnostic.Create(descriptor, location, items));

    public static Accessibility GetMinVisibility(this IReadOnlyList<ITypeSymbol> typeSymbols)
    {
        var inputTypeAccess = typeSymbols[0].GetVisibility();
        var accessibility = Accessibility.Public;
        var oneOrMoreOfTheOutputTypesIsInternal = false;

        for (var i = 1; i < typeSymbols.Count; ++i)
        {
            var typeAccess = typeSymbols[i].GetVisibility();
            if (typeAccess < accessibility)
            {
                accessibility = typeAccess;
            }

            if (typeAccess == Accessibility.Internal)
            {
                oneOrMoreOfTheOutputTypesIsInternal = true;
            }
        }

        if (inputTypeAccess == Accessibility.Protected && oneOrMoreOfTheOutputTypesIsInternal && accessibility > Accessibility.Private)
        {
            accessibility = Accessibility.Internal;
        }

        return accessibility;
    }

    public static Accessibility GetVisibility(this ITypeSymbol typeSymbol)
    {
        var accessibility = GetVisibilityHelper(typeSymbol);
        if (accessibility == Accessibility.Protected && typeSymbol.DeclaredAccessibility == Accessibility.Internal)
        {
            return Accessibility.Internal;
        }

        return accessibility;
    }

    private static Accessibility GetVisibilityHelper(this ITypeSymbol typeSymbol)
    {
        var accessibility = typeSymbol.DeclaredAccessibility;
        typeSymbol = typeSymbol.ContainingType;

        while (typeSymbol is not null)
        {
            if (typeSymbol.DeclaredAccessibility < accessibility)
            {
                accessibility = typeSymbol.DeclaredAccessibility;
            }

            typeSymbol = typeSymbol.ContainingType;
        }

        return accessibility;
    }
}
