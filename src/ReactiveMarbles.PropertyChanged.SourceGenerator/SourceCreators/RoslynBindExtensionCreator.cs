// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static ReactiveMarbles.PropertyChanged.SourceGenerator.SyntaxFactoryHelpers;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    internal class RoslynBindExtensionCreator : ISourceCreator
    {
        public string Create(IEnumerable<IDatum> sources)
        {
            var members = sources.Cast<BindInvocationInfo>().Select(x => Create(x)).ToList();

            if (members.Count > 0)
            {
                var compilation = CompilationUnit(default, members, RoslynHelpers.GetReactiveExtensionUsings());

                return compilation.ToFullString();
            }

            return null;
        }

        private static ClassDeclarationSyntax Create(BindInvocationInfo classDatum)
        {
            var visibility = new[] { SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword, SyntaxKind.PartialKeyword };

            // generates the BindExtensions with Bind methods overload.
            return ClassDeclaration("BindExtensions", visibility, Create(classDatum.ViewModelArgument, classDatum.ViewArgument, classDatum.Accessibility, classDatum.HasConverters).ToList(), 1);
        }

        private static IEnumerable<MemberDeclarationSyntax> Create(ExpressionArgument viewModelArgument, ExpressionArgument viewArgument, Accessibility accessibility, bool hasConverters)
        {
            var statements = new List<StatementSyntax>();

            var viewModelInputType = viewModelArgument.InputType.ToDisplayString();
            var viewModelOutputType = viewModelArgument.OutputType.ToDisplayString();

            var viewInputType = viewArgument.InputType.ToDisplayString();
            var viewOutputType = viewArgument.OutputType.ToDisplayString();

            // generates: var hostObs = fromObject.WhenChanged(fromProperty);
            statements.Add(RoslynHelpers.InvokeWhenChangedVariable(viewModelOutputType, "hostObs", "fromProperty", "fromObject"));

            // generates: var targetObs = targetObject.WhenChanged(toProperty).Skip(1);
            statements.Add(RoslynHelpers.InvokeWhenChangedSkipVariable(viewModelOutputType, "targetObs", "toProperty", "targetObject", 1));

            if (hasConverters)
            {
                // generates: hostObs = hostObs.Select(hostToTargetConv);
                statements.Add(ExpressionStatement(
                    AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        "hostObs",
                        InvocationExpression(
                            MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, "hostObs", "Select"), new[] { Argument("hostToTargetConv") }))));

                // generates: targetObs = targetObs.Select(targetToHostConv);
                statements.Add(ExpressionStatement(
                    AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        "targetObs",
                        InvocationExpression(
                            MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, "targetObs", "Select"), new[] { Argument("targetToHostConv") }))));
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

                        // generates: targetObs = targetObs.ObserveOn(scheduler);
                        ExpressionStatement(
                            AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                "targetObs",
                                InvocationExpression(
                                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, "targetObs", "ObserveOn"), new[] { Argument("scheduler") }))),
                    },
                    1));

            // generates: return new CompositeDisposable(...);
            statements.Add(ReturnStatement(ObjectCreationExpression(
                "CompositeDisposable",
                new[]
                {
                    // generates: hostObs.Subscribe(x => targetObject.[propertyName] = x);
                    Argument(InvocationExpression(
                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, "hostObs", "Subscribe"),
                        new[]
                        {
                            Argument(SimpleLambdaExpression(
                                Parameter("x"),
                                AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, "targetObject", viewArgument.ExpressionChain[viewArgument.ExpressionChain.Count - 1].Name),
                                    "x")))
                        })),

                    // generates: targetObs.Subscribe(x => fromObject.[propertyName] = x);
                    Argument(InvocationExpression(
                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, "targetObs", "Subscribe"),
                        new[]
                        {
                            Argument(SimpleLambdaExpression(
                                Parameter("x"),
                                AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, "fromObject", viewModelArgument.ExpressionChain[viewModelArgument.ExpressionChain.Count - 1].Name),
                                    "x")))
                        }))
                })));

            // generates: Bind() method.
            yield return RoslynHelpers.Bind(viewModelInputType, viewModelOutputType, viewInputType, viewOutputType, true, hasConverters, accessibility, Block(statements, 1));
        }
    }
}
