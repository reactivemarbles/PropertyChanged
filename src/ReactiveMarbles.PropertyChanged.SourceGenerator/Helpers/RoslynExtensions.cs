// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Helpers;

internal static class RoslynExtensions
{
    public static IEnumerable<SyntaxKind> GetAccessibilityTokens(this Accessibility accessibility) => accessibility switch
    {
        Accessibility.Public => new[] { SyntaxKind.PublicKeyword },
        Accessibility.Internal => new[] { SyntaxKind.InternalKeyword },
        Accessibility.Private => new[] { SyntaxKind.PrivateKeyword },
        Accessibility.NotApplicable => Array.Empty<SyntaxKind>(),
        Accessibility.ProtectedAndInternal => new[] { SyntaxKind.PrivateKeyword, SyntaxKind.ProtectedKeyword },
        Accessibility.Protected => new[] { SyntaxKind.ProtectedKeyword },
        Accessibility.ProtectedOrInternal => new[] { SyntaxKind.ProtectedKeyword, SyntaxKind.InternalKeyword },
        _ => Array.Empty<SyntaxKind>(),
    };

    public static string ToNamespaceName(this INamespaceSymbol symbol)
    {
        var name = symbol.ToDisplayString();

        return name.Equals("<global namespace>") ? string.Empty : name;
    }
}
