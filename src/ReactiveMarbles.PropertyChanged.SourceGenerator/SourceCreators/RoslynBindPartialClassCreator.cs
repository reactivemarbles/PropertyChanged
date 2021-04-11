// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static ReactiveMarbles.PropertyChanged.SourceGenerator.SyntaxFactoryHelpers;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    internal class RoslynBindPartialClassCreator : ISourceCreator
    {
        public string Create(IEnumerable<IDatum> sources)
        {
            var members = sources.Cast<BindInvocationInfo>().Select(x => Create(x)).ToList();

            if (members.Count > 0)
            {
                var compilation = CompilationUnit(default, members, RoslynHelpers.GetReactiveExtensionUsings());

                return compilation.ToFullString();
            }

            return null;
        }

        private static ClassDeclarationSyntax Create(BindInvocationInfo classDatum)
        {
            var visibility = new[] { SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword, SyntaxKind.PartialKeyword };

            return default;
        }
    }
}
