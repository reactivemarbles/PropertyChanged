﻿// Copyright (c) 2019-2020 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ReactiveMarbles.PropertyChanged
{
    internal class SyntaxReceiver : ISyntaxReceiver
    {
        public List<InvocationExpressionSyntax> WhenChangedMethods { get; } = new();

        /// <inheritdoc />
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is InvocationExpressionSyntax invocationExpression)
            {
                var methodName = (invocationExpression.Expression as MemberAccessExpressionSyntax)?.Name.ToString() ??
                    (invocationExpression.Expression as MemberBindingExpressionSyntax)?.Name.ToString();

                if (string.Equals(methodName, "WhenChanged"))
                {
                    WhenChangedMethods.Add(invocationExpression);
                }
            }
        }
    }
}
