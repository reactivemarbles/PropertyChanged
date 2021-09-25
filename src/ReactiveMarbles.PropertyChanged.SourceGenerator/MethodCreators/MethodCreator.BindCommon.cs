// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using ReactiveMarbles.PropertyChanged.SourceGenerator.Comparers;
using ReactiveMarbles.RoslynHelpers;

using static ReactiveMarbles.RoslynHelpers.SyntaxFactoryHelpers;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.MethodCreators
{
    internal partial class MethodCreator
    {
        private static (HashSet<MethodDatum> Extensions, HashSet<MethodDatum> Partials) GenerateBind(IReadOnlyList<InvocationExpressionSyntax> invocations, CSharpCompilation compilation, string bindMethodName, Func<ExpressionArgument, ExpressionArgument, bool, bool, List<ExpressionChain>, List<ExpressionChain>, List<StatementSyntax>> createBindStatements, Func<string, string, string, string, bool, bool, Accessibility, List<StatementSyntax>, MethodDeclarationSyntax> methodGenerator, in GeneratorExecutionContext context)
        {
            var values = new Dictionary<(bool IsExtension, string NamespaceName, string HostClassName, Accessibility Accessibility, SemanticModel SemanticModel), List<IfStatementSyntax>>();
            var extensions = new Dictionary<(ExpressionArgument HostArgument, ExpressionArgument TargetArgument, bool HasConverters, Accessibility Accessibility, SemanticModel Model), ISet<IfStatementSyntax>>();
            var partials = new Dictionary<(ExpressionArgument HostArgument, ExpressionArgument TargetArgument, bool HasConverters, Accessibility Accessibility, SemanticModel Model), ISet<IfStatementSyntax>>();

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

                    if (!methodSymbol.ContainingType.ToDisplayString().Equals(Constants.BindExtensionClass))
                    {
                        continue;
                    }

                    if (!methodSymbol.Name.Equals(bindMethodName))
                    {
                        continue;
                    }

                    if (!GetExpressions(invocationExpression, model, compilation, context, out var hostExpressionArgument, out var targetExpressionArgument, out var hasConverters, out var hostExpressionChains, out var targetExpressionChains))
                    {
                        continue;
                    }

                    if (hostExpressionArgument is null || targetExpressionArgument is null || hostExpressionChains is null || targetExpressionChains is null)
                    {
                        context.ReportDiagnostic(DiagnosticWarnings.InvalidExpression, invocationExpression.GetLocation());
                        continue;
                    }

                    var accessModifier = methodSymbol.TypeArguments.GetMinVisibility();

                    var isExtension = !hostExpressionArgument.ContainsPrivateOrProtectedMember && !targetExpressionArgument.ContainsPrivateOrProtectedMember;

                    var statements = createBindStatements(hostExpressionArgument, targetExpressionArgument, hasConverters, isExtension, hostExpressionChains, targetExpressionChains);

                    var namespaceName = hostExpressionArgument.InputType.GetNamespace();
                    var hostClassName = hostExpressionArgument.InputType.ToDisplayString(RoslynCommonHelpers.TypeFormat);
                    var accessibility = hostExpressionArgument.InputType.DeclaredAccessibility;
                    var semanticModel = compilation.GetSemanticModel(hostExpressionArgument.InputType.Locations[0].SourceTree ?? throw new InvalidOperationException("There is no valid source tree")) ?? throw new InvalidOperationException("There is no valid location.");

                    var dictionary = isExtension ? extensions : partials;
                    if (!dictionary.TryGetValue((hostExpressionArgument, targetExpressionArgument, hasConverters, accessibility, model), out var ifStatements))
                    {
                        ifStatements = new SortedSet<IfStatementSyntax>(SyntaxNodeComparer.Default);
                        dictionary.Add((hostExpressionArgument, targetExpressionArgument, hasConverters, accessibility, model), ifStatements);
                    }

                    ifStatements.Add(
                        IfStatement(
                            InvocationExpression(
                                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, Constants.StringTypeName, Constants.EqualsMethod),
                                new[]
                                {
                                    Argument(InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, Constants.FromObjectVariable, Constants.ToStringMethod))),
                                    Argument(LiteralExpression(hostExpressionArgument.LambdaBodyString))
                                }),
                            Block(statements, 1)));
                }
            }

            return (GenerateMethods(extensions, true, methodGenerator), GenerateMethods(partials, false, methodGenerator));
        }

        private static HashSet<MethodDatum> GenerateMethods(Dictionary<(ExpressionArgument HostArgument, ExpressionArgument TargetArgument, bool HasConverters, Accessibility Accessibility, SemanticModel Model), ISet<IfStatementSyntax>> input, bool isExtension, Func<string, string, string, string, bool, bool, Accessibility, List<StatementSyntax>, MethodDeclarationSyntax> methodGenerator)
        {
            var hashSet = new HashSet<MethodDatum>();
            foreach (var kvp in input)
            {
                var hostInputType = kvp.Key.HostArgument.InputType;
                var hostOutputType = kvp.Key.HostArgument.OutputType;
                var targetInputType = kvp.Key.TargetArgument.InputType;
                var targetOutputType = kvp.Key.TargetArgument.OutputType;
                var hasConverters = kvp.Key.HasConverters;
                var accessibility = kvp.Key.Accessibility;
                var model = kvp.Key.Model;

                var statements = kvp.Value;

                var method = methodGenerator(
                    hostInputType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    hostOutputType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    targetInputType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    targetOutputType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    isExtension,
                    hasConverters,
                    accessibility,
                    statements.Cast<StatementSyntax>().ToList());

                hashSet.Add(new MethodDatum(isExtension, string.Empty, Constants.BindExtensionClass, accessibility, model, method));
            }

            return hashSet;
        }

        private static bool GetExpressions(InvocationExpressionSyntax invocationExpression, SemanticModel model, Compilation compilation, in GeneratorExecutionContext context, out ExpressionArgument? hostExpressionArgument, out ExpressionArgument? targetExpressionArgument, out bool hasConverters, out List<ExpressionChain>? hostExpressionChains, out List<ExpressionChain>? targetExpressionChains)
        {
            hasConverters = false;
            var expressions = new List<(LambdaExpressionSyntax Expression, ArgumentSyntax Argument)>();

            hostExpressionArgument = null!;
            targetExpressionArgument = null!;
            hostExpressionChains = null;
            targetExpressionChains = null;

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

            if (!GeneratorHelpers.GetExpression(context, targetExpression, compilation, model, out targetExpressionArgument, out targetExpressionChains))
            {
                // The argument is evaluates to an expression but it's not inline (could be a variable, method invocation, etc).
                context.ReportDiagnostic(DiagnosticWarnings.InvalidExpression, targetExpression.GetLocation());
                return false;
            }

            if (!GeneratorHelpers.GetExpression(context, sourceExpression, compilation, model, out hostExpressionArgument, out hostExpressionChains))
            {
                // The argument is evaluates to an expression but it's not inline (could be a variable, method invocation, etc).
                context.ReportDiagnostic(DiagnosticWarnings.InvalidExpression, sourceExpression.GetLocation());
                return false;
            }

            return true;
        }
    }
}
