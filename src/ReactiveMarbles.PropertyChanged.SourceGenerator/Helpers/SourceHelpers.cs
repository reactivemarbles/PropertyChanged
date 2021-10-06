// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static ReactiveMarbles.RoslynHelpers.SyntaxFactoryHelpers;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator.Helpers;

internal static class SourceHelpers
{
    /// <summary>
    /// Generates:
    /// [source].WhenChanged(expressionName).
    /// </summary>
    /// <param name="methodName">The method name.</param>
    /// <param name="expressionName">The expression.</param>
    /// <param name="source">The source variable.</param>
    /// <returns>The invocation.</returns>
    public static InvocationExpressionSyntax InvokeWhenChanged(string methodName, string expressionName, string source) =>
        InvocationExpression(
            MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                IdentifierName(source),
                methodName),
            new[]
            {
                Argument(expressionName),
                Argument(Constants.CallerMemberParameterName),
                Argument(Constants.CallerFilePathParameterName),
                Argument(Constants.CallerLineNumberParameterName),
            });
}