// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static ReactiveMarbles.RoslynHelpers.SyntaxFactoryHelpers;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    internal class RoslynBindTwoWayExtensionCreator : RoslynBindBase
    {
        public override string? Create(IEnumerable<IDatum> sources)
        {
            var members = sources.Cast<BindInvocationInfo>().Select(Create).ToList();

            if (members.Count > 0)
            {
                var compilation = CompilationUnit(default, members, RoslynHelpers.GetReactiveExtensionUsingDirectives());

                return compilation.ToFullString();
            }

            return null;
        }

        private static ClassDeclarationSyntax Create(BindInvocationInfo classDatum)
        {
            var visibility = new[] { SyntaxKind.InternalKeyword, SyntaxKind.StaticKeyword, SyntaxKind.PartialKeyword };

            // generates the BindExtensions with Bind methods overload.
            return ClassDeclaration("BindExtensions", visibility, CreateBind(classDatum.ViewModelArgument, classDatum.ViewArgument, classDatum.Accessibility, classDatum.HasConverters, true).ToList(), 1);
        }
    }
}
