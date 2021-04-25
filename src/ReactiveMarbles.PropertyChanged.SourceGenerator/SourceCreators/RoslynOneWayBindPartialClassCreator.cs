// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static ReactiveMarbles.PropertyChanged.SourceGenerator.SyntaxFactoryHelpers;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    internal class RoslynOneWayBindPartialClassCreator : RoslynBindBase
    {
        public override string Create(IEnumerable<IDatum> sources)
        {
            var members = new List<MemberDeclarationSyntax>();

            foreach (var group in sources
                .OfType<PartialOneWayBindInvocationInfo>()
                .GroupBy(x => x.NamespaceName))
            {
                var classes = group.Select(Create).ToList();
                if (!string.IsNullOrWhiteSpace(group.Key))
                {
                    var groupNamespace = NamespaceDeclaration(group.Key, classes, true);
                    members.Add(groupNamespace);
                }
                else
                {
                    members.AddRange(classes);
                }
            }

            if (members.Count == 0)
            {
                return null;
            }

            var compilation = CompilationUnit(default, members, RoslynHelpers.GetReactiveExtensionUsings());

            return compilation.ToFullString();
        }

        private static ClassDeclarationSyntax Create(PartialOneWayBindInvocationInfo classDatum)
        {
            var visibility = classDatum.ViewModelArgument.InputType.DeclaredAccessibility.GetAccessibilityTokens().Concat(new[] { SyntaxKind.PartialKeyword }).ToList();

            var currentClass = ClassDeclaration(classDatum.ClassName, visibility, CreateBind(classDatum.ViewModelArgument, classDatum.ViewArgument, classDatum.Accessibility, classDatum.HasConverters, false).ToList(), 1);

            foreach (var ancestor in classDatum.AncestorClasses)
            {
                visibility = ancestor.AccessModifier.GetAccessibilityTokens().Concat(new[] { SyntaxKind.PartialKeyword }).ToList();
                currentClass = ClassDeclaration(ancestor.Name, visibility, new[] { currentClass }, 0);
            }

            return currentClass;
        }
    }
}
