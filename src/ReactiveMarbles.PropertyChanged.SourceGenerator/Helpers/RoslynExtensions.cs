// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
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
        Accessibility.Public => [SyntaxKind.PublicKeyword],
        Accessibility.Internal => [SyntaxKind.InternalKeyword],
        Accessibility.Private => [SyntaxKind.PrivateKeyword],
        Accessibility.NotApplicable => [],
        Accessibility.ProtectedAndInternal => [SyntaxKind.PrivateKeyword, SyntaxKind.ProtectedKeyword],
        Accessibility.Protected => [SyntaxKind.ProtectedKeyword],
        Accessibility.ProtectedOrInternal => [SyntaxKind.ProtectedKeyword, SyntaxKind.InternalKeyword],
        _ => Array.Empty<SyntaxKind>(),
    };

    public static string ToNamespaceName(this INamespaceSymbol symbol)
    {
        var name = symbol.ToDisplayString();

        return name.Equals("<global namespace>") ? string.Empty : name;
    }
}
