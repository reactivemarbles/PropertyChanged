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

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Tests
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

        public static SyntaxTokenList GetToken(this Accessibility accessibility) => accessibility switch
        {
            Accessibility.Public => TokenList(Token(SyntaxKind.PublicKeyword)),
            Accessibility.Internal => TokenList(Token(SyntaxKind.InternalKeyword)),
            Accessibility.Private => TokenList(Token(SyntaxKind.PrivateKeyword)),
            Accessibility.NotApplicable => TokenList(),
            Accessibility.ProtectedAndInternal => TokenList(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.ProtectedKeyword)),
            Accessibility.Protected => TokenList(Token(SyntaxKind.ProtectedKeyword)),
            Accessibility.ProtectedOrInternal => TokenList(Token(SyntaxKind.ProtectedKeyword), Token(SyntaxKind.InternalKeyword)),
            _ => TokenList(),
        };
    }
}
