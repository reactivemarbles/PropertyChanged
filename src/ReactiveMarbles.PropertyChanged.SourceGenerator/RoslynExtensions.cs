// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    internal static class RoslynExtensions
    {
        public static SyntaxList<MemberDeclarationSyntax> Concat<TInput, TOther>(this IEnumerable<TInput> input, params IEnumerable<TOther>[] others)
            where TInput : MemberDeclarationSyntax
            where TOther : MemberDeclarationSyntax
        {
            var members = input.Cast<MemberDeclarationSyntax>().ToList();

            foreach (var other in others)
            {
                foreach (var item in other.Where(x => x != null))
                {
                    members.AddRange(other);
                }
            }

            if (members.Count > 0)
            {
                return List(members);
            }

            return List<MemberDeclarationSyntax>();
        }

        public static IEnumerable<SyntaxToken> GetAccessibilityTokens(this Accessibility accessibility) => accessibility switch
        {
            Accessibility.Public => new[] { Token(SyntaxKind.PublicKeyword) },
            Accessibility.Internal => new[] { Token(SyntaxKind.InternalKeyword) },
            Accessibility.Private => new[] { Token(SyntaxKind.PrivateKeyword) },
            Accessibility.NotApplicable => new SyntaxToken[] { },
            Accessibility.ProtectedAndInternal => new[] { Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.ProtectedKeyword) },
            Accessibility.Protected => new[] { Token(SyntaxKind.ProtectedKeyword) },
            Accessibility.ProtectedOrInternal => new[] { Token(SyntaxKind.ProtectedKeyword), Token(SyntaxKind.InternalKeyword) },
            _ => new SyntaxToken[] { },
        };

        public static CompilationUnitSyntax WithStandardReactiveUsings(this CompilationUnitSyntax compilation) =>
            compilation.WithUsings(List(
                    new[]
                    {
                        UsingDirective(IdentifierName("System")),
                        UsingDirective(IdentifierName("System.Collections.Generic")),
                        UsingDirective(IdentifierName("System.ComponentModel")),
                        UsingDirective(IdentifierName("System.Linq.Expressions")),
                        UsingDirective(IdentifierName("System.Reactive.Disposables")),
                        UsingDirective(IdentifierName("System.Reactive.Linq")),
                        UsingDirective(IdentifierName("System.Runtime.CompilerServices")),
                    }));
    }
}
