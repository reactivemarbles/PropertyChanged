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

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.SourceCreator
{
    internal class RoslynWhenChangedPartialClassCreator : ISourceCreator
    {
        public string Create(IEnumerable<IDatum> sourceData)
        {
            var members = new List<MemberDeclarationSyntax>();

            foreach (var group in sourceData
                .Cast<PartialClassDatum>()
                .GroupBy(x => x.NamespaceName))
            {
                var classes = group.Select(x => Create(x));
                if (!string.IsNullOrWhiteSpace(group.Key))
                {
                    var groupNamespace = NamespaceDeclaration(IdentifierName(group.Key))
                        .WithMembers(List<MemberDeclarationSyntax>(classes));
                    members.Add(groupNamespace);
                }
                else
                {
                    members.AddRange(members);
                }
            }

            var compilation = CompilationUnit()
                .WithStandardReactiveUsings()
                .WithMembers(List(members));

            return compilation.NormalizeWhitespace().ToFullString();
        }

        private static ClassDeclarationSyntax Create(PartialClassDatum classDatum)
        {
            var visibility = classDatum.AccessModifier.GetAccessibilityTokens().Concat(new[] { Token(SyntaxKind.PartialKeyword) });
            return ClassDeclaration(classDatum.Name)
                .WithMembers(List(classDatum.MethodData.SelectMany(x => Create(x))))
                .WithModifiers(TokenList(visibility));
        }

        private static IEnumerable<MemberDeclarationSyntax> Create(MethodDatum method) =>
            method switch
            {
                SingleExpressionDictionaryImplMethodDatum methodDatum => Create(methodDatum),
                SingleExpressionOptimizedImplMethodDatum methodDatum => Create(methodDatum),
                MultiExpressionMethodDatum methodDatum => Create(methodDatum),
                _ => throw new InvalidOperationException("Unknown type of datum."),
            };

        private static IEnumerable<MemberDeclarationSyntax> Create(SingleExpressionDictionaryImplMethodDatum methodDatum)
        {
            var mapEntries = new List<AssignmentExpressionSyntax>();

            foreach (var entry in methodDatum.Map.Entries)
            {
                var observable = RoslynHelpers.GetObservableChain("this", entry.Members);
                mapEntries.Add(RoslynHelpers.MapEntry(entry.Key, observable));
            }

            yield return RoslynHelpers.MapDictionary(methodDatum.InputTypeName, methodDatum.OutputTypeName, methodDatum.Map.MapName, mapEntries);
            yield return RoslynHelpers.WhenChangedWithoutBody(methodDatum.InputTypeName, methodDatum.OutputTypeName, false, methodDatum.AccessModifier)
                .WithExpressionBody(ArrowExpressionClause(RoslynHelpers.MapInvokeExpression("this", methodDatum.Map.MapName, "propertyExpression")));
        }

        private static IEnumerable<MemberDeclarationSyntax> Create(SingleExpressionOptimizedImplMethodDatum methodDatum)
        {
            yield return RoslynHelpers.WhenChangedWithoutBody(methodDatum.InputTypeName, methodDatum.OutputTypeName, false, methodDatum.AccessModifier)
                .WithExpressionBody(ArrowExpressionClause(RoslynHelpers.GetObservableChain("this", methodDatum.Members)));
        }

        private static IEnumerable<MemberDeclarationSyntax> Create(MultiExpressionMethodDatum methodDatum)
        {
            ////var dictionaryCalls = Enumerable.Range(1, methodDatum.TempReturnTypes.Count).Select(x => RoslynHelpers.MapInvokeExpression())
            ////var whenChanged = RoslynHelpers.WhenChangedConversionWithoutBody(methodDatum.InputType.ToDisplayString(), methodDatum.OutputType.ToDisplayString(), methodDatum.TempReturnTypes, false, methodDatum.AccessModifier)
            ////    .WithExpressionBody(ArrowExpressionClause(;

            return Enumerable.Empty<MemberDeclarationSyntax>();
        }
    }
}
