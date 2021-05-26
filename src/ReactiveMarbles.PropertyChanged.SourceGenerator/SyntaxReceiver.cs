// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ReactiveMarbles.PropertyChanged.SourceGenerator
{
    internal class SyntaxReceiver : ISyntaxReceiver
    {
        public List<InvocationExpressionSyntax> WhenChangedMethods { get; } = new();

        public List<InvocationExpressionSyntax> WhenChangingMethods { get; } = new();

        public List<InvocationExpressionSyntax> BindMethods { get; } = new();

        public List<InvocationExpressionSyntax> OneWayBindMethods { get; } = new();

        /// <inheritdoc />
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is not InvocationExpressionSyntax invocationExpression)
            {
                return;
            }

            var methodName = (invocationExpression.Expression as MemberAccessExpressionSyntax)?.Name.ToString() ??
                             (invocationExpression.Expression as MemberBindingExpressionSyntax)?.Name.ToString();

            if (string.Equals(methodName, Constants.WhenChangedMethodName))
            {
                WhenChangedMethods.Add(invocationExpression);
            }

            if (string.Equals(methodName, Constants.WhenChangingMethodName))
            {
                WhenChangingMethods.Add(invocationExpression);
            }

            if (string.Equals(methodName, Constants.BindMethodName))
            {
                BindMethods.Add(invocationExpression);
            }

            if (string.Equals(methodName, Constants.OneWayBindMethodName))
            {
                OneWayBindMethods.Add(invocationExpression);
            }
        }
    }
}
