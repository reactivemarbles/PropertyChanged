// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using ReactiveMarbles.PropertyChanged.SourceGenerator.Helpers;
using ReactiveMarbles.PropertyChanged.SourceGenerator.MethodCreators.Transient;

using static ReactiveMarbles.RoslynHelpers.SyntaxFactoryHelpers;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.MethodCreators;

internal static partial class MethodCreator
{
    private static List<StatementSyntax> CreateTwoWayBindStatements(in ExpressionArgument hostExpressionArgument, in ExpressionArgument targetExpressionArgument, bool hasConverters, bool isExtension)
    {
        var statements = new List<StatementSyntax>();

        var hostOutputType = hostExpressionArgument.OutputType.ToDisplayString();

        var targetOutputType = targetExpressionArgument.OutputType.ToDisplayString();

        var fromName = isExtension ? Constants.FromObjectVariable : Constants.ThisObjectVariable;

        var hostObservableChain = SourceHelpers.InvokeWhenChanged(Constants.WhenChangedMethodName, Constants.FromPropertyParameter, fromName);
        statements.Add(LocalDeclarationStatement(VariableDeclaration(GenericName(Constants.IObservableTypeName, new[] { IdentifierName(hostOutputType) }), new[] { VariableDeclarator(Constants.HostObservableVariable, EqualsValueClause(hostObservableChain)) })));

        var targetObservableChainInside = SourceHelpers.InvokeWhenChanged(Constants.WhenChangedMethodName, Constants.ToPropertyParameter, Constants.TargetParameter); ////GetObservableChain(fromName, targetExpressionChains, Constants.WhenChangedEventName, Constants.WhenChangedEventHandler, isExtension ? 2 : 3);
        var targetObservableChain = InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, Constants.ObservableLinqTypeName, Constants.SkipMethodName), new[] { Argument(targetObservableChainInside), Argument(LiteralExpression(1)) });
        statements.Add(LocalDeclarationStatement(VariableDeclaration(GenericName(Constants.IObservableTypeName, new[] { IdentifierName(targetOutputType) }), new[] { VariableDeclarator(Constants.TargetObservableVariable, EqualsValueClause(targetObservableChain)) })));

        if (hasConverters)
        {
            // generates: hostObs = hostObs.Select(hostToTargetConv);
            statements.Add(ExpressionStatement(
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    Constants.HostObservableVariable,
                    InvocationExpression(
                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, Constants.HostObservableVariable, Constants.SelectMethod), new[] { Argument(Constants.HostToTargetConverterFuncParameter) }))));

            // generates: targetObs = targetObs.Select(targetToHostConv);
            statements.Add(ExpressionStatement(
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    Constants.TargetObservableVariable,
                    InvocationExpression(
                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, Constants.TargetObservableVariable, Constants.SelectMethod), new[] { Argument(Constants.TargetToHostConverterFuncParameter) }))));
        }

        // generates: scheduler = scheduler ?? ImmediateScheduler.Instance;
        statements.Add(ExpressionStatement(
            AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                Constants.SchedulerParameter,
                BinaryExpression(
                    SyntaxKind.CoalesceExpression,
                    Constants.SchedulerParameter,
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        Constants.ImmediateSchedulerTypeName,
                        Constants.InstancePropertyName)))));

        // generates: if (scheduler != ImmediateScheduler.Instance) { ... }
        IfStatement(
            BinaryExpression(
                SyntaxKind.NotEqualsExpression,
                Constants.SchedulerParameter,
                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, Constants.ImmediateSchedulerTypeName, Constants.InstancePropertyName)),
            Block(
                new[]
                {
                    // generates: hostObs = hostObs.ObserveOn(scheduler);
                    ExpressionStatement(
                        AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            Constants.HostObservableVariable,
                            InvocationExpression(
                                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, Constants.HostObservableVariable, Constants.ObserveOnMethodName), new[] { Argument(Constants.SchedulerParameter) }))),

                    // generates: targetObs = targetObs.ObserveOn(scheduler);
                    ExpressionStatement(
                        AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            Constants.TargetObservableVariable,
                            InvocationExpression(
                                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, Constants.TargetObservableVariable, Constants.ObserveOnMethodName), new[] { Argument(Constants.SchedulerParameter) }))),
                },
                1));

        // generates: return new CompositeDisposable(...);
        statements.Add(ReturnStatement(ObjectCreationExpression(
            Constants.CompositeDisposableTypeName,
            new[]
            {
                // generates: hostObs.Subscribe(x => targetObject.[propertyName] = x);
                Argument(InvocationExpression(
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, Constants.ObservableExtensionsTypeName, Constants.SubscribeMethodName),
                    new[]
                    {
                        Argument(Constants.HostObservableVariable),
                        Argument(SimpleLambdaExpression(
                            Parameter(Constants.LambdaSingleParameterName),
                            AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, Constants.TargetParameter, targetExpressionArgument.ExpressionChain[targetExpressionArgument.ExpressionChain.Count - 1].Name),
                                Constants.LambdaSingleParameterName))),
                    })),

                // generates: targetObs.Subscribe(x => fromObject.[propertyName] = x);
                Argument(InvocationExpression(
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, Constants.ObservableExtensionsTypeName, Constants.SubscribeMethodName),
                    new[]
                    {
                        Argument(Constants.TargetObservableVariable),
                        Argument(SimpleLambdaExpression(
                            Parameter(Constants.LambdaSingleParameterName),
                            AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, fromName, hostExpressionArgument.ExpressionChain[hostExpressionArgument.ExpressionChain.Count - 1].Name),
                                Constants.LambdaSingleParameterName))),
                    })),
            })));

        // generates: Bind() method.
        return statements;
    }

    private static MethodDeclarationSyntax CreateBindTwoWayMethod(string hostInputType, string hostOutputType, string targetInputType, string targetOutputType, bool isExtension, bool hasConverters, Accessibility accessibility, List<StatementSyntax> statements)
    {
        var modifiers = accessibility.GetAccessibilityTokens().ToList();

        var parameterList = new List<ParameterSyntax>();

        if (isExtension)
        {
            modifiers.Add(SyntaxKind.StaticKeyword);
            parameterList.Add(Parameter(hostInputType, Constants.FromObjectVariable, new[] { SyntaxKind.ThisKeyword }));
        }

        parameterList.Add(Parameter(targetInputType, Constants.TargetParameter));

        parameterList.Add(Parameter(GetExpressionFunc(hostInputType, hostOutputType), Constants.FromPropertyParameter));
        parameterList.Add(Parameter(GetExpressionFunc(targetInputType, targetOutputType), Constants.ToPropertyParameter));

        if (hasConverters)
        {
            parameterList.Add(Parameter(GenericName(Constants.FuncTypeName, new[] { IdentifierName(hostInputType), IdentifierName(targetOutputType) }), Constants.HostToTargetConverterFuncParameter));
            parameterList.Add(Parameter(GenericName(Constants.FuncTypeName, new[] { IdentifierName(targetInputType), IdentifierName(hostOutputType) }), Constants.TargetToHostConverterFuncParameter));
        }

        parameterList.Add(Parameter(Constants.ISchedulerTypeName, Constants.SchedulerParameter, EqualsValueClause(NullLiteral())));

        parameterList.AddRange(CallerMembersParameters());

        statements.Add(ThrowStatement(ObjectCreationExpression(Constants.InvalidOperationExceptionTypeName, new[] { Argument("\"No valid expression found.\"") }), isExtension ? 2 : 3));

        var body = Block(statements, isExtension ? 1 : 2);
        return MethodDeclaration(GetMethodAttributes(), modifiers, Constants.SystemDisposableTypeName, Constants.BindTwoWayMethodName, parameterList, isExtension ? 1 : 2, body);
    }
}