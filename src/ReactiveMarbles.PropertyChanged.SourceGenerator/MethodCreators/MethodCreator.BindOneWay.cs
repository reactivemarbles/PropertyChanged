// Copyright (c) 2019-2025 ReactiveUI Association Incorporated. All rights reserved.
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
    private static List<StatementSyntax> CreateOneWayBindStatements(in ExpressionArgument hostExpressionArgument, in ExpressionArgument targetExpressionArgument, bool hasConverters, bool isExtension)
    {
        var statements = new List<StatementSyntax>();

        var hostOutputType = hostExpressionArgument.OutputType.ToDisplayString();

        var fromName = isExtension ? Constants.FromObjectVariable : Constants.ThisObjectVariable;

        // generates: var hostObs = fromObject.WhenChanged(fromProperty);
        var observableChain = SourceHelpers.InvokeWhenChanged(Constants.WhenChangedMethodName, Constants.FromPropertyParameter, fromName);

        statements.Add(LocalDeclarationStatement(VariableDeclaration(GenericName(Constants.IObservableTypeName, [IdentifierName(hostOutputType)]), new[] { VariableDeclarator(Constants.HostObservableVariable, EqualsValueClause(observableChain)) })));

        if (hasConverters)
        {
            // generates: hostObs = hostObs.Select(hostToTargetConv);
            statements.Add(ExpressionStatement(
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    Constants.HostObservableVariable,
                    InvocationExpression(
                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, Constants.HostObservableVariable, Constants.SelectMethod),
                        [Argument(Constants.HostToTargetConverterFuncParameter)]))));
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
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    Constants.HostObservableVariable,
                                    Constants.ObserveOnMethodName),
                                [Argument(Constants.SchedulerParameter)]))),
                },
                isExtension ? 3 : 4));

        statements.Add(ReturnStatement(InvocationExpression(
            MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, Constants.ObservableExtensionsTypeName, Constants.SubscribeMethodName),
            new[]
            {
                Argument(Constants.HostObservableVariable),
                Argument(SimpleLambdaExpression(
                    Parameter(Constants.LambdaSingleParameterName),
                    AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            Constants.TargetParameter,
                            targetExpressionArgument.ExpressionChain[targetExpressionArgument.ExpressionChain.Count - 1].Name),
                        Constants.LambdaSingleParameterName))),
            })));

        return statements;
    }

    private static MethodDeclarationSyntax CreateBindOneWayMethod(
        string hostInputType,
        string hostOutputType,
        string targetInputType,
        string targetOutputType,
        bool isExtension,
        bool hasConverters,
        Accessibility accessibility,
        List<StatementSyntax> statements)
    {
        var modifiers = accessibility.GetAccessibilityTokens().ToList();

        var parameterList = new List<ParameterSyntax>();

        if (isExtension)
        {
            modifiers.Add(SyntaxKind.StaticKeyword);
            parameterList.Add(Parameter(hostInputType, Constants.FromObjectVariable, [SyntaxKind.ThisKeyword]));
        }

        parameterList.Add(Parameter(targetInputType, Constants.TargetParameter));

        parameterList.Add(Parameter(GetExpressionFunc(hostInputType, hostOutputType), Constants.FromPropertyParameter));
        parameterList.Add(Parameter(GetExpressionFunc(targetInputType, targetOutputType), Constants.ToPropertyParameter));

        if (hasConverters)
        {
            parameterList.Add(
                Parameter(
                    GenericName(
                        Constants.FuncTypeName,
                        [IdentifierName(hostInputType), IdentifierName(targetOutputType)]),
                    Constants.HostToTargetConverterFuncParameter));
        }

        parameterList.Add(Parameter(Constants.ISchedulerTypeName, Constants.SchedulerParameter, EqualsValueClause(NullLiteral())));

        parameterList.AddRange(CallerMembersParameters());

        statements.Add(ThrowStatement(ObjectCreationExpression(Constants.InvalidOperationExceptionTypeName, [Argument("\"No valid expression found.\"")]), isExtension ? 2 : 3));

        var body = Block(statements, isExtension ? 1 : 2);

        return MethodDeclaration(GetMethodAttributes(), modifiers, Constants.SystemDisposableTypeName, Constants.BindOneWayMethodName, parameterList, 1, body);
    }
}
