// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using ReactiveMarbles.PropertyChanged.SourceGenerator.Comparers;
using ReactiveMarbles.PropertyChanged.SourceGenerator.Helpers;
using ReactiveMarbles.PropertyChanged.SourceGenerator.MethodCreators.Transient;
using ReactiveMarbles.RoslynHelpers;

using static ReactiveMarbles.RoslynHelpers.SyntaxFactoryHelpers;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.MethodCreators;

internal static partial class MethodCreator
{
    private delegate List<StatementSyntax> CreateBindFunc(in ExpressionArgument host, in ExpressionArgument target, bool hasConverters, bool isExtension);

    private delegate MethodDeclarationSyntax CreateBindMethodFunc(string hostInputType, string hostOutputType, string targetInputType, string targetOutputType, bool isExtension, bool hasConverters, Accessibility accessibility, List<StatementSyntax> statements);

    private static void GenerateBind(IReadOnlyList<InvocationExpressionSyntax> invocations, Compilation compilation, string bindMethodName, ICollection<MultiWhenStatementsDatum> whenChanged, CompilationDatum compilationData, in GeneratorExecutionContext context)
    {
        var extensionStatements = new Dictionary<BindStatementsDatum, SortedSet<IfStatementSyntax>>();
        var partialStatements = new Dictionary<BindStatementsDatum, SortedSet<IfStatementSyntax>>();

        var isOneWayBind = bindMethodName.Equals(Constants.BindOneWayMethodName, StringComparison.Ordinal);
        CreateBindFunc createBindStatements = isOneWayBind ? CreateOneWayBindStatements : CreateTwoWayBindStatements;
        CreateBindMethodFunc methodGenerator = isOneWayBind ? CreateBindOneWayMethod : CreateBindTwoWayMethod;

        foreach (var invocationExpression in invocations)
        {
            var model = compilation.GetSemanticModel(invocationExpression.SyntaxTree);
            var symbol = model.GetSymbolInfo(invocationExpression).Symbol;
            if (symbol is not IMethodSymbol methodSymbol)
            {
                continue;
            }

            if (!methodSymbol.ContainingType.ToDisplayString().Equals(Constants.BindExtensionClass))
            {
                continue;
            }

            if (!methodSymbol.Name.Equals(bindMethodName))
            {
                continue;
            }

            if (!GetBindExpressions(invocationExpression, model, compilation, context, out var hostExpressionArgument, out var targetExpressionArgument, out var hasConverters))
            {
                continue;
            }

            if (hostExpressionArgument == default || targetExpressionArgument == default)
            {
                context.ReportDiagnostic(DiagnosticWarnings.InvalidExpression, invocationExpression.GetLocation());
                continue;
            }

            var accessModifier = methodSymbol.TypeArguments.GetMinVisibility();

            var isExtension = !hostExpressionArgument.ContainsPrivateOrProtectedMember && !targetExpressionArgument.ContainsPrivateOrProtectedMember;

            var statements = createBindStatements(hostExpressionArgument, targetExpressionArgument, hasConverters, isExtension);

            var hostClassAccessibility = hostExpressionArgument.InputType.DeclaredAccessibility;
            var targetClassAccessibility = targetExpressionArgument.InputType.DeclaredAccessibility;

            var dictionary = isExtension ? extensionStatements : partialStatements;
            var statementKey = new BindStatementsDatum(hostExpressionArgument, targetExpressionArgument, bindMethodName, hasConverters, hostClassAccessibility, accessModifier, model);
            if (!dictionary.TryGetValue(statementKey, out var ifStatements))
            {
                ifStatements = new(SyntaxNodeComparer.Default);
                dictionary.Add(statementKey, ifStatements);
            }

            whenChanged.Add(new(Constants.WhenChangedMethodName, new[] { new WhenStatementsDatum(Constants.WhenChangedMethodName, hostExpressionArgument) }, !hostExpressionArgument.ContainsPrivateOrProtectedMember, hostClassAccessibility, hostExpressionArgument.InputType, hostExpressionArgument.OutputType, new[] { hostExpressionArgument.InputType, hostExpressionArgument.OutputType }, methodSymbol));
            whenChanged.Add(new(Constants.WhenChangedMethodName, new[] { new WhenStatementsDatum(Constants.WhenChangedMethodName, targetExpressionArgument) }, !targetExpressionArgument.ContainsPrivateOrProtectedMember, targetClassAccessibility, targetExpressionArgument.InputType, targetExpressionArgument.OutputType, new[] { targetExpressionArgument.InputType, targetExpressionArgument.OutputType }, methodSymbol));

            ifStatements.Add(
                IfStatement(
                    BinaryExpression(
                        SyntaxKind.LogicalAndExpression,
                        InvocationExpression(
                            MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, Constants.StringTypeName, Constants.EqualsMethod),
                            new[]
                            {
                                Argument(InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, Constants.FromPropertyParameter, Constants.ToStringMethod))),
                                Argument(LiteralExpression(hostExpressionArgument.LambdaBodyString))
                            }),
                        InvocationExpression(
                            MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, Constants.StringTypeName, Constants.EqualsMethod),
                            new[]
                            {
                                Argument(InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, Constants.ToPropertyParameter, Constants.ToStringMethod))),
                                Argument(LiteralExpression(hostExpressionArgument.LambdaBodyString))
                            })),
                    Block(statements, isExtension ? 2 : 3)));
        }

        GenerateBindMethods(extensionStatements, true, methodGenerator, compilationData);
        GenerateBindMethods(partialStatements, false, methodGenerator, compilationData);
    }

    private static void GenerateBindMethods(Dictionary<BindStatementsDatum, SortedSet<IfStatementSyntax>> input, bool isExtension, CreateBindMethodFunc methodGenerator, CompilationDatum compilationData)
    {
        foreach (var kvp in input)
        {
            var hostInputType = kvp.Key.HostArgument.InputType;
            var hostOutputType = kvp.Key.HostArgument.OutputType;
            var targetInputType = kvp.Key.TargetArgument.InputType;
            var targetOutputType = kvp.Key.TargetArgument.OutputType;
            var hasConverters = kvp.Key.HasConverters;
            var methodAccessibility = kvp.Key.MethodAccessibility;
            var classAccessibility = kvp.Key.ClassAccessibilty;
            var statements = kvp.Value;

            var method = methodGenerator(
                hostInputType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                hostOutputType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                targetInputType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                targetOutputType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                isExtension,
                hasConverters,
                methodAccessibility,
                statements.Cast<StatementSyntax>().ToList());

            var classDatum = compilationData.GetClass(hostInputType, classAccessibility, isExtension, Constants.BindExtensionClass);

            var methodMetadata = new MethodDatum(methodAccessibility, method);

            classDatum.MethodData.Add(methodMetadata);
        }
    }

    private static bool GetBindExpressions(InvocationExpressionSyntax invocationExpression, SemanticModel model, Compilation compilation, in GeneratorExecutionContext context, out ExpressionArgument? hostExpressionArgument, out ExpressionArgument? targetExpressionArgument, out bool hasConverters)
    {
        hasConverters = false;
        var expressions = new List<(LambdaExpressionSyntax Expression, ArgumentSyntax Argument)>();

        hostExpressionArgument = default!;
        targetExpressionArgument = default!;

        if (invocationExpression.ArgumentList.Arguments.Count < 3)
        {
            context.ReportDiagnostic(DiagnosticWarnings.BindingIncorrectNumberParameters, invocationExpression.GetLocation());
            return false;
        }

        foreach (var argument in invocationExpression.ArgumentList.Arguments.Skip(1))
        {
            var argumentType = model.GetTypeInfo(argument.Expression).ConvertedType;

            if (argumentType is null)
            {
                context.ReportDiagnostic(DiagnosticWarnings.InvalidExpression, argument.GetLocation());
                return false;
            }

            if (argumentType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).StartsWith(Constants.FuncTypeName, StringComparison.InvariantCulture))
            {
                hasConverters = true;
                continue;
            }

            if (argumentType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).StartsWith(Constants.ExpressionTypeName, StringComparison.InvariantCulture) && argument.Expression is LambdaExpressionSyntax lambdaExpression)
            {
                expressions.Add((lambdaExpression, argument));
            }
        }

        if (expressions.Count != 2)
        {
            context.ReportDiagnostic(DiagnosticWarnings.InvalidNumberExpressions, invocationExpression.GetLocation(), expressions.Count.ToString());
            return false;
        }

        var sourceExpression = expressions[0].Expression;

        if (sourceExpression is null)
        {
            // The argument is evaluates to an expression but it's not inline (could be a variable, method invocation, etc).
            context.ReportDiagnostic(DiagnosticWarnings.ExpressionMustBeInline, expressions[0].Argument.GetLocation());
            return false;
        }

        var targetExpression = expressions[1].Expression;

        if (targetExpression is null)
        {
            // The argument is evaluates to an expression but it's not inline (could be a variable, method invocation, etc).
            context.ReportDiagnostic(DiagnosticWarnings.ExpressionMustBeInline, expressions[1].Argument.GetLocation());
            return false;
        }

        if (!GeneratorHelpers.GetExpression(context, targetExpression, compilation, model, out targetExpressionArgument))
        {
            // The argument is evaluates to an expression but it's not inline (could be a variable, method invocation, etc).
            context.ReportDiagnostic(DiagnosticWarnings.InvalidExpression, targetExpression.GetLocation());
            return false;
        }

        if (!GeneratorHelpers.GetExpression(context, sourceExpression, compilation, model, out hostExpressionArgument))
        {
            // The argument is evaluates to an expression but it's not inline (could be a variable, method invocation, etc).
            context.ReportDiagnostic(DiagnosticWarnings.InvalidExpression, sourceExpression.GetLocation());
            return false;
        }

        return true;
    }
}
