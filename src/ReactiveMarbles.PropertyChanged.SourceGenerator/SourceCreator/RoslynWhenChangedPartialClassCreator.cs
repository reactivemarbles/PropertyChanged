// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static ReactiveMarbles.PropertyChanged.SourceGenerator.SyntaxFactoryHelpers;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    internal class RoslynWhenChangedPartialClassCreator : ISourceCreator
    {
        public string Create(IEnumerable<IDatum> sources)
        {
            var members = new List<MemberDeclarationSyntax>();

            foreach (var group in sources
                .Cast<PartialClassDatum>()
                .GroupBy(x => x.NamespaceName))
            {
                var classes = group.Select(x => Create(x)).ToList();
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

            if (members.Count > 0)
            {
                var compilation = CompilationUnit(default, members, RoslynHelpers.GetReactiveExtensionUsings());

                return compilation.ToFullString();
            }

            return null;
        }

        private static ClassDeclarationSyntax Create(PartialClassDatum classDatum)
        {
            var visibility = classDatum.AccessModifier.GetAccessibilityTokens().Concat(new[] { SyntaxKind.PartialKeyword }).ToList();

            var currentClass = ClassDeclaration(classDatum.Name, visibility, classDatum.MethodData.SelectMany(x => Create(x)).ToList(), 1);

            foreach (var ancestor in classDatum.AncestorClasses)
            {
                visibility = ancestor.AccessModifier.GetAccessibilityTokens().Concat(new[] { SyntaxKind.PartialKeyword }).ToList();
                currentClass = ClassDeclaration(ancestor.Name, visibility, new[] { currentClass }, 0);
            }

            return currentClass;
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
            yield return RoslynHelpers.WhenChanged(methodDatum.InputTypeName, methodDatum.OutputTypeName, false, methodDatum.AccessModifier, ArrowExpressionClause(RoslynHelpers.MapInvokeExpression("this", methodDatum.Map.MapName, "propertyExpression")));
        }

        private static IEnumerable<MemberDeclarationSyntax> Create(SingleExpressionOptimizedImplMethodDatum methodDatum)
        {
            yield return RoslynHelpers.WhenChanged(methodDatum.InputTypeName, methodDatum.OutputTypeName, false, methodDatum.AccessModifier, ArrowExpressionClause(RoslynHelpers.GetObservableChain("this", methodDatum.Members)));
        }

        private static IEnumerable<MemberDeclarationSyntax> Create(MultiExpressionMethodDatum methodDatum)
        {
            var statements = new List<StatementSyntax>(methodDatum.TempReturnTypes.Count);
            var combineArguments = new List<ArgumentSyntax>(methodDatum.TempReturnTypes.Count);

            for (int i = 0; i < methodDatum.TempReturnTypes.Count; ++i)
            {
                var type = methodDatum.TempReturnTypes[i];
                var obsName = "obs" + (i + 1);
                var whenChangedVariable = LocalDeclarationStatement(VariableDeclaration($"IObservable<{type}>", new[] { VariableDeclarator(obsName, EqualsValueClause(RoslynHelpers.InvokeWhenChanged("propertyExpression" + (i + 1), "this"))) }));
                statements.Add(whenChangedVariable);
                combineArguments.Add(Argument(obsName));
            }

            combineArguments.Add(Argument("conversionFunc"));

            statements.Add(ReturnStatement(
                InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        "Observable",
                        "CombineLatest"),
                    combineArguments)));

            yield return RoslynHelpers.WhenChangedConversion(methodDatum.InputType.ToDisplayString(), methodDatum.OutputType.ToDisplayString(), methodDatum.TempReturnTypes, false, methodDatum.AccessModifier, Block(statements, 1));
        }
    }
}
