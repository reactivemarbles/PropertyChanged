// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static ReactiveMarbles.RoslynHelpers.SyntaxFactoryHelpers;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    internal abstract class RoslynOneWayBindBase : ISourceCreator
    {
        public abstract string Create(IEnumerable<IDatum> sources);

        protected static IEnumerable<MemberDeclarationSyntax> CreateOneWayBind(ExpressionArgument hostArgument, ExpressionArgument targetArgument, Accessibility accessibility, bool hasConverters, bool isExtension)
        {
            var statements = new List<StatementSyntax>();

            var viewModelInputType = hostArgument.InputType.ToDisplayString();
            var viewModelOutputType = hostArgument.OutputType.ToDisplayString();

            var viewInputType = targetArgument.InputType.ToDisplayString();
            var viewOutputType = targetArgument.OutputType.ToDisplayString();

            var fromName = isExtension ? "fromObject" : "this";

            // generates: var hostObs = fromObject.WhenChanged(fromProperty);
            statements.Add(RoslynHelpers.InvokeWhenChangedVariable("WhenChanged", viewModelOutputType, "hostObs", "fromProperty", fromName));

            if (hasConverters)
            {
                // generates: hostObs = hostObs.Select(hostToTargetConv);
                statements.Add(ExpressionStatement(
                    AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        "hostObs",
                        InvocationExpression(
                            MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, "hostObs", "Select"), new[] { Argument("hostToTargetConv") }))));
            }

            // generates: scheduler = scheduler ?? ImmediateScheduler.Instance;
            statements.Add(ExpressionStatement(
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    "scheduler",
                    BinaryExpression(
                        SyntaxKind.CoalesceExpression,
                        "scheduler",
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            "ImmediateScheduler",
                            "Instance")))));

            // generates: if (scheduler != ImmediateScheduler.Instance) { ... }
            IfStatement(
                BinaryExpression(
                    SyntaxKind.NotEqualsExpression,
                    "scheduler",
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, "ImmediateScheduler", "Instance")),
                Block(
                    new[]
                    {
                        // generates: hostObs = hostObs.ObserveOn(scheduler);
                        ExpressionStatement(
                            AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                "hostObs",
                                InvocationExpression(
                                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, "hostObs", "ObserveOn"), new[] { Argument("scheduler") }))),
                    },
                    1));

            statements.Add(ReturnStatement(InvocationExpression(
                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, "hostObs", "Subscribe"),
                        new[]
                        {
                            Argument(SimpleLambdaExpression(
                                Parameter("x"),
                                AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, "targetObject", targetArgument.ExpressionChain[targetArgument.ExpressionChain.Count - 1].Name),
                                    "x"))),
                        })));

            // generates: Bind() method.
            ////yield return RoslynHelpers.OneWayBind(viewModelInputType, viewModelOutputType, viewInputType, viewOutputType, isExtension, hasConverters, accessibility, Block(statements, 1));

            return Enumerable.Empty<MemberDeclarationSyntax>();
        }
    }
}
