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

using ReactiveMarbles.PropertyChanged.SourceGenerator.MethodCreators;
using ReactiveMarbles.PropertyChanged.SourceGenerator.RoslynHelpers;
using ReactiveMarbles.RoslynHelpers;

using static ReactiveMarbles.RoslynHelpers.SyntaxFactoryHelpers;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.MethodCreators
{
    internal partial class MethodCreator
    {
        private static HashSet<MethodDatum> GenerateWhen(IReadOnlyList<InvocationExpressionSyntax> invocations, CSharpCompilation compilation, string methodClass, Func<ExpressionArgument, bool, bool, Accessibility, MethodDeclarationSyntax> createFunc, in GeneratorExecutionContext context)
        {
            var returnValue = new HashSet<MethodDatum>();

            foreach (var invocationExpressionGrouping in invocations.GroupBy(x => compilation.GetSemanticModel(x.SyntaxTree)))
            {
                var model = invocationExpressionGrouping.Key;
                foreach (var invocationExpression in invocationExpressionGrouping)
                {
                    var symbol = model.GetSymbolInfo(invocationExpression).Symbol;

                    if (symbol is not IMethodSymbol methodSymbol)
                    {
                        continue;
                    }

                    if (!methodSymbol.ContainingType.ToDisplayString().Equals(methodClass))
                    {
                        continue;
                    }

                    var invocationArguments = invocationExpression.ArgumentList.Arguments.Where(argument => model.GetTypeInfo(argument.Expression).ConvertedType?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Equals(Constants.ExpressionTypeName) == true);

                    foreach (var argument in invocationArguments)
                    {
                        if (argument.Expression is LambdaExpressionSyntax lambdaExpression)
                        {
                            var isValid = GeneratorHelpers.GetExpression(context, lambdaExpression, compilation, model, out var expressionArgument, out var expressionChains);

                            if (!isValid)
                            {
                                continue;
                            }

                            if (expressionArgument is null)
                            {
                                continue;
                            }

                            // this.WhenChanged(...) and instance.WhenChanged(...) MethodKind = MethodKind.ReducedExtension
                            // NotifyPropertyChangedExtensions.WhenChanged(...) MethodKind = MethodKind.Ordinary
                            // An alternative way is checking if methodSymbol.ReceiverType.Name == NotifyPropertyChangedExtensions.
                            var isExplicitInvocation = methodSymbol.MethodKind == MethodKind.Ordinary;

                            if (isExplicitInvocation && expressionArgument.ContainsPrivateOrProtectedMember)
                            {
                                context.ReportDiagnostic(DiagnosticWarnings.UnableToGenerateExtension, invocationExpression.GetLocation());
                                break;
                            }

                            var accessModifier = methodSymbol.TypeArguments.GetMinVisibility();

                            var isExtension = !expressionArgument.ContainsPrivateOrProtectedMember;

                            var method = createFunc(expressionArgument, isExplicitInvocation, isExtension, accessModifier);

                            var namespaceName = expressionArgument.InputType.GetNamespace();
                            var hostClassName = expressionArgument.InputType.ToDisplayString(RoslynCommonHelpers.TypeFormat);

                            ////returnValue.Add(new MethodDatum(isExtension, namespaceName, hostClassName, accessModifier, method));
                        }
                        else
                        {
                            // The argument is evaluates to an expression but it's not inline (could be a variable, method invocation, etc).
                            context.ReportDiagnostic(DiagnosticWarnings.ExpressionMustBeInline, argument.GetLocation());
                        }
                    }
                }
            }

            return returnValue;
        }

        private static MethodDeclarationSyntax CreateWhenMethod(
            string methodName,
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
                parameterList.Add(Parameter(inputType, Constants.SourceParameterName));
            }

            parameterList.Add(Parameter(
                    GetExpressionFunc(inputType, outputType),
                    Constants.PropertyExpressionParameterName));

            parameterList.AddRange(CallerMembersParameters());

            var body = Block(statements, 1);

            var returnType = GenericName(Constants.IObservableTypeName, new[] { IdentifierName(outputType) });
            return MethodDeclaration(modifiers, returnType, methodName, parameterList, 0, body);
        }

        private static InvocationExpressionSyntax GetObservableChain(string inputName, IReadOnlyList<ExpressionChain> members, string eventName, string handlerName)
        {
            InvocationExpressionSyntax? observable = null;
            for (var i = 0; i < members.Count; ++i)
            {
                var (name, _, outputType) = members[i];

                observable = i == 0 || observable is null ?
                    ObservableNotifyPropertyChanged(outputType.ToDisplayString(), inputName, name, eventName, handlerName) :
                    SelectObservableNotifyPropertyChangedSwitch(observable, outputType.ToDisplayString(), Constants.SourceParameterName, name, eventName, handlerName);
            }

            return observable!;
        }

        private static InvocationExpressionSyntax ObservableNotifyPropertyChanged(string returnType, string inputName, string memberName, string eventName, string handlerName) =>
            InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName(Constants.ObservableLinqTypeName),
                    GenericName(Constants.CreateMethodName, new TypeSyntax[] { IdentifierName(returnType) })),
                new[]
                {
                    Argument(
                        SimpleLambdaExpression(
                            Parameter(Constants.ObserverParameterName),
                            Block(
                                new StatementSyntax[]
                                {
                                    IfStatement(
                                        BinaryExpression(SyntaxKind.EqualsExpression, IdentifierName(inputName), NullLiteral()),
                                        ReturnStatement(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, Constants.ReactiveDisposableTypeName, Constants.EmptyPropertyName))),
                                    ExpressionStatement(InvocationExpression(
                                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, Constants.ObserverParameterName, Constants.OnNextMethodName),
                                        new[] { Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, inputName, memberName)) })),
                                    LocalDeclarationStatement(
                                        VariableDeclaration(
                                            handlerName,
                                            new[]
                                            {
                                                VariableDeclarator(
                                                    Constants.HandlerParameterName,
                                                    EqualsValueClause(ParenthesizedLambdaExpression(
                                                        new[]
                                                        {
                                                            Parameter(Constants.SenderParameterName),
                                                            Parameter(Constants.EventArgumentsParameterName),
                                                        },
                                                        Block(
                                                            new StatementSyntax[]
                                                            {
                                                                IfStatement(
                                                                    BinaryExpression(
                                                                        SyntaxKind.EqualsExpression,
                                                                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, Constants.EventArgumentsParameterName, Constants.PropertyNamePropertyName),
                                                                        LiteralExpression(memberName)),
                                                                    Block(
                                                                        new StatementSyntax[]
                                                                        {
                                                                            ExpressionStatement(InvocationExpression(
                                                                                MemberAccessExpression(
                                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                                    Constants.ObserverParameterName,
                                                                                    Constants.OnNextMethodName),
                                                                                new[]
                                                                                {
                                                                                    Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, inputName, memberName)),
                                                                                })),
                                                                        },
                                                                        3)),
                                                            },
                                                            2)))),
                                            })),
                                    ExpressionStatement(AssignmentExpression(SyntaxKind.AddAssignmentExpression, MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, inputName, eventName), IdentifierName("handler"))),
                                    ReturnStatement(InvocationExpression(
                                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, Constants.ReactiveDisposableTypeName, Constants.CreateMethodName),
                                        new[]
                                        {
                                            Argument(TupleExpression(
                                                new[]
                                                {
                                                    Argument(inputName, Constants.ParentPropertyName),
                                                    Argument(Constants.HandlerParameterName, Constants.HandlerMethodName),
                                                })),
                                            Argument(SimpleLambdaExpression(
                                                Parameter(Constants.LambdaSingleParameterName),
                                                AssignmentExpression(
                                                    SyntaxKind.SubtractAssignmentExpression,
                                                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, inputName, eventName),
                                                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, Constants.LambdaSingleParameterName, Constants.HandlerMethodName)))),
                                        })),
                                },
                                1))),
                });

        private static InvocationExpressionSyntax SelectObservableNotifyPropertyChangedSwitch(InvocationExpressionSyntax sourceInvoke, string returnType, string inputName, string memberName, string eventName, string handlerName) =>
            InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            sourceInvoke,
                            Constants.SelectMethod),
                        new[]
                        {
                            Argument(SimpleLambdaExpression(Parameter(inputName), ObservableNotifyPropertyChanged(returnType, inputName, memberName, eventName, handlerName))),
                        }),
                    Constants.SwitchMethodName));
    }
}
