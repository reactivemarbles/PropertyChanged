// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using ReactiveMarbles.PropertyChanged.SourceGenerator.MethodCreators;
using ReactiveMarbles.PropertyChanged.SourceGenerator.RoslynHelpers;

using static ReactiveMarbles.RoslynHelpers.SyntaxFactoryHelpers;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.MethodCreators
{
    internal partial class MethodCreator
    {
        private static List<StatementSyntax> CreateWhenChangedStatements(bool isExtension, List<ExpressionChain> expressionChain)
        {
            var statements = new List<StatementSyntax>();

            var fromName = isExtension ? Constants.FromObjectVariable : Constants.ThisObjectVariable;

            // generates: var hostObs = fromObject.WhenChanged(fromProperty);
            var observableChain = GetObservableChain(fromName, expressionChain, Constants.WhenChangedEventName, Constants.WhenChangedEventHandler);

            statements.Add(ExpressionStatement(observableChain));

            return statements;
        }

        private static MethodDeclarationSyntax CreateWhenChangedMethod(
            string inputType,
            string outputType,
            bool isExplicit,
            bool isExtension,
            Accessibility accessibility,
            List<StatementSyntax> statements)
        {
            var modifiers = accessibility.GetAccessibilityTokens().ToList();

            var parameterList = new List<ParameterSyntax>();

            if (isExtension && !isExplicit)
            {
                modifiers.Add(SyntaxKind.StaticKeyword);
                parameterList.Add(Parameter(inputType, Constants.SourceParameterName, new[] { SyntaxKind.ThisKeyword }));
            }
            else if (isExtension && isExplicit)
            {
                parameterList.Add(Parameter(inputType, Constants.SourceParameterName, new[] { SyntaxKind.ThisKeyword }));
            }

            parameterList.Add(Parameter(
                    GetExpressionFunc(inputType, outputType),
                    Constants.PropertyExpressionParameterName));

            parameterList.AddRange(CallerMembersParameters());

            var body = Block(statements, 1);

            var returnType = GenericName(Constants.IObservableTypeName, new[] { IdentifierName(outputType) });
            return MethodDeclaration(modifiers, returnType, Constants.WhenChangedMethodName, parameterList, 0, body);
        }
    }
}
