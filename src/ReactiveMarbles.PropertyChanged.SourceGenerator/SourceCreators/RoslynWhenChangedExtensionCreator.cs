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
    internal class RoslynWhenChangedExtensionCreator : ISourceCreator
    {
        private string _className;
        private string _methodName;
        private string _eventName;
        private string _handlerName;

        private RoslynWhenChangedExtensionCreator(
            string className,
            string methodName,
            string eventName,
            string handlerName)
        {
            _className = className;
            _methodName = methodName;
            _eventName = eventName;
            _handlerName = handlerName;
        }

        public string MethodName => _methodName;

        public static RoslynWhenChangedExtensionCreator WhenChanging()
        {
            return new("NotifyPropertyChangingExtensions", Constants.WhenChangingMethodName, "PropertyChanging", "PropertyChangingEventHandler");
        }

        public static RoslynWhenChangedExtensionCreator WhenChanged()
        {
            return new("NotifyPropertyChangedExtensions", Constants.WhenChangedMethodName, "PropertyChanged", "PropertyChangedEventHandler");
        }

        public string Create(IEnumerable<IDatum> sources)
        {
            var members = sources.Cast<ExtensionClassDatum>().Select(Create).ToList();

            if (members.Count > 0)
            {
                var compilation = CompilationUnit(default, members, RoslynHelpers.GetReactiveExtensionUsings());

                return compilation.ToFullString();
            }

            return null;
        }

        private ClassDeclarationSyntax Create(ExtensionClassDatum classDatum)
        {
            var visibility = new[] { SyntaxKind.InternalKeyword, SyntaxKind.StaticKeyword, SyntaxKind.PartialKeyword };
            return ClassDeclaration(_className, visibility, classDatum.MethodData.SelectMany(Create).ToList(), 1);
        }

        private IEnumerable<MemberDeclarationSyntax> Create(MethodDatum method) =>
            method switch
            {
                SingleExpressionDictionaryImplMethodDatum methodDatum => Create(methodDatum),
                SingleExpressionOptimizedImplMethodDatum methodDatum => Create(methodDatum),
                MultiExpressionMethodDatum methodDatum => Create(methodDatum),
                _ => throw new InvalidOperationException("Unknown type of datum."),
            };

        private IEnumerable<MemberDeclarationSyntax> Create(SingleExpressionDictionaryImplMethodDatum methodDatum)
        {
            var mapEntries = new List<AssignmentExpressionSyntax>();

            foreach (var (key, entries) in methodDatum.Map.Entries)
            {
                var observable = RoslynHelpers.GetObservableChain("source", entries, _eventName, _handlerName);
                mapEntries.Add(RoslynHelpers.MapEntry(key, observable));
            }

            yield return RoslynHelpers.MapDictionary(methodDatum.InputTypeName, methodDatum.OutputTypeName, methodDatum.Map.MapName, mapEntries);
            yield return RoslynHelpers.WhenChanged(_methodName, methodDatum.InputTypeName, methodDatum.OutputTypeName, true, methodDatum.AccessModifier, ArrowExpressionClause(RoslynHelpers.MapInvokeExpression("source", methodDatum.Map.MapName, "propertyExpression")));
        }

        private IEnumerable<MemberDeclarationSyntax> Create(SingleExpressionOptimizedImplMethodDatum methodDatum)
        {
            yield return RoslynHelpers.WhenChanged(_methodName, methodDatum.InputTypeName, methodDatum.OutputTypeName, true, methodDatum.AccessModifier, ArrowExpressionClause(RoslynHelpers.GetObservableChain("source", methodDatum.Members, _eventName, _handlerName)));
        }

        private IEnumerable<MemberDeclarationSyntax> Create(MultiExpressionMethodDatum methodDatum)
        {
            var statements = new List<StatementSyntax>(methodDatum.TempReturnTypes.Count);
            var combineArguments = new List<ArgumentSyntax>(methodDatum.TempReturnTypes.Count);

            for (var i = 0; i < methodDatum.TempReturnTypes.Count; ++i)
            {
                var type = methodDatum.TempReturnTypes[i];
                var obsName = "obs" + (i + 1);
                var whenChangedVariable = RoslynHelpers.InvokeWhenChangedVariable(_methodName, type, obsName, "propertyExpression" + (i + 1), "source");
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

            yield return RoslynHelpers.WhenChangedConversion(_methodName, methodDatum.InputType.ToDisplayString(), methodDatum.OutputType.ToDisplayString(), methodDatum.TempReturnTypes, true, methodDatum.AccessModifier, Block(statements, 1));
        }
    }
}
